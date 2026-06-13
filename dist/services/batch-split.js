"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.BatchSplitService = void 0;
const client_1 = require("@prisma/client");
const logger_1 = __importDefault(require("./logger"));
const prisma = new client_1.PrismaClient();
class BatchSplitService {
    async splitOrders() {
        logger_1.default.info('Starting batch split process');
        const orders = await prisma.amazonOrder.findMany({
            where: { orderStatus: 'Unshipped', batches: { none: {} } },
            include: { items: true },
        });
        logger_1.default.info(`Found ${orders.length} orders to split`);
        const results = [];
        for (const order of orders) {
            try {
                const result = await this.splitSingleOrder(order);
                if (result)
                    results.push(result);
            }
            catch (error) {
                logger_1.default.error('Failed to split order', {
                    amazonOrderId: order.amazonOrderId,
                    error: error.message,
                });
            }
        }
        return results;
    }
    async splitSingleOrder(order) {
        const { amazonOrderId, items } = order;
        if (items.length === 0)
            return null;
        const uniqueSkus = new Set(items.map((i) => i.sellerSKU));
        if (items.length === 1) {
            return this.createBatch(amazonOrderId, this.getBatchTypeForQuantity(items[0].quantityOrdered), null, items);
        }
        if (uniqueSkus.size === 1) {
            const totalQty = items.reduce((sum, i) => sum + i.quantityOrdered, 0);
            return this.createBatch(amazonOrderId, this.getBatchTypeForQuantity(totalQty), items[0].sellerSKU, items);
        }
        return this.createBatch(amazonOrderId, 'MIXED', null, items);
    }
    getBatchTypeForQuantity(qty) {
        if (qty === 1)
            return 'SINGLE_SKU_Q1';
        if (qty === 2)
            return 'SINGLE_SKU_Q2';
        return 'SINGLE_SKU_Q3PLUS';
    }
    async createBatch(amazonOrderId, batchType, batchGroup, items) {
        const group = batchGroup ? this.determineBatchGroup(batchGroup) : null;
        const batch = await prisma.orderBatch.create({
            data: {
                amazonOrderId,
                batchType,
                batchGroup: group,
                items: {
                    create: items.map((i) => ({
                        orderItemId: i.id,
                        sku: i.sellerSKU,
                        quantity: i.quantityOrdered,
                    })),
                },
            },
            include: { items: true },
        });
        return {
            orderId: batch.id,
            amazonOrderId,
            batchType,
            batchGroup: group,
            items: items.map((i) => ({
                orderItemId: i.orderItemId,
                sku: i.sellerSKU,
                quantity: i.quantityOrdered,
            })),
        };
    }
    determineBatchGroup(sku) {
        return sku.split('-')[0] || sku;
    }
    async getBatchesByType(batchType) {
        const where = batchType ? { batchType } : {};
        return prisma.orderBatch.findMany({
            where,
            include: { order: { include: { items: true } }, items: true },
            orderBy: { createdAt: 'desc' },
        });
    }
    async getBatchStatistics() {
        const batches = await prisma.orderBatch.groupBy({
            by: ['batchType'],
            _count: { batchType: true },
        });
        const breakdown = {};
        for (const b of batches) {
            breakdown[b.batchType] = b._count.batchType;
        }
        const totalUnshippedOrders = await prisma.amazonOrder.count({
            where: { orderStatus: 'Unshipped' },
        });
        const splitOrderIds = await prisma.orderBatch.findMany({
            select: { amazonOrderId: true },
            distinct: ['amazonOrderId'],
        });
        return {
            totalUnshippedOrders,
            totalBatchesCreated: splitOrderIds.length,
            breakdown,
        };
    }
}
exports.BatchSplitService = BatchSplitService;
//# sourceMappingURL=batch-split.js.map