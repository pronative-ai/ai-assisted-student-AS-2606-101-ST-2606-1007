"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.OrderSyncService = void 0;
const client_1 = require("@prisma/client");
const logger_1 = __importDefault(require("./logger"));
const prisma = new client_1.PrismaClient();
class OrderSyncService {
    spApiClient;
    constructor(spApiClient) {
        this.spApiClient = spApiClient;
    }
    async syncUnshippedOrders() {
        const syncLog = await prisma.syncLog.create({
            data: { syncType: 'ORDER_SYNC', status: 'SUCCESS', startedAt: new Date() },
        });
        try {
            const createdAfter = await this.getLastSyncTime();
            const allOrders = [];
            let nextToken;
            do {
                const response = nextToken
                    ? await this.spApiClient.getOrdersNextPage(nextToken)
                    : await this.spApiClient.getUnshippedOrders(createdAfter);
                if (response.Orders)
                    allOrders.push(...response.Orders);
                nextToken = response.NextToken;
            } while (nextToken);
            logger_1.default.info(`Found ${allOrders.length} unshipped orders`);
            let syncedCount = 0;
            let totalItems = 0;
            for (const order of allOrders) {
                try {
                    await this.upsertOrder(order);
                    syncedCount++;
                    totalItems += await this.syncOrderItems(order.AmazonOrderId);
                }
                catch (orderError) {
                    logger_1.default.error('Failed to sync order', {
                        amazonOrderId: order.AmazonOrderId,
                        error: orderError.message,
                    });
                }
            }
            const status = syncedCount === allOrders.length ? 'SUCCESS' : 'PARTIAL';
            await prisma.syncLog.update({
                where: { id: syncLog.id },
                data: { status, ordersFound: allOrders.length, ordersSynced: syncedCount, itemsSynced: totalItems, completedAt: new Date() },
            });
            return { syncId: syncLog.id, ordersFound: allOrders.length, ordersSynced: syncedCount, itemsSynced: totalItems, status };
        }
        catch (error) {
            logger_1.default.error('Order sync failed', { error: error.message });
            await prisma.syncLog.update({
                where: { id: syncLog.id },
                data: { status: 'FAILED', errorMessage: error.message, completedAt: new Date() },
            });
            return { syncId: syncLog.id, ordersFound: 0, ordersSynced: 0, itemsSynced: 0, status: 'FAILED', error: error.message };
        }
    }
    async getLastSyncTime() {
        const lastSync = await prisma.syncLog.findFirst({
            where: { syncType: 'ORDER_SYNC', status: { in: ['SUCCESS', 'PARTIAL'] } },
            orderBy: { startedAt: 'desc' },
        });
        if (lastSync?.startedAt)
            return lastSync.startedAt.toISOString();
        const d = new Date();
        d.setDate(d.getDate() - 30);
        return d.toISOString();
    }
    async upsertOrder(order) {
        await prisma.amazonOrder.upsert({
            where: { amazonOrderId: order.AmazonOrderId },
            create: {
                amazonOrderId: order.AmazonOrderId,
                purchaseDate: new Date(order.PurchaseDate),
                orderStatus: order.OrderStatus,
                fulfillmentChannel: order.FulfillmentChannel,
                marketplaceId: order.MarketplaceId,
                buyerEmail: order.BuyerEmail ?? null,
                buyerName: order.BuyerName ?? null,
                shippingAddress: order.ShippingAddress ? JSON.stringify(order.ShippingAddress) : null,
                orderTotal: order.OrderTotal ? parseFloat(order.OrderTotal.Amount) : null,
                currencyCode: order.OrderTotal?.CurrencyCode ?? null,
                numberOfItemsShipped: order.NumberOfItemsShipped ?? 0,
                numberOfItemsUnshipped: order.NumberOfItemsUnshipped ?? 0,
                lastSyncAt: new Date(),
            },
            update: {
                orderStatus: order.OrderStatus,
                buyerEmail: order.BuyerEmail ?? null,
                buyerName: order.BuyerName ?? null,
                shippingAddress: order.ShippingAddress ? JSON.stringify(order.ShippingAddress) : null,
                orderTotal: order.OrderTotal ? parseFloat(order.OrderTotal.Amount) : null,
                currencyCode: order.OrderTotal?.CurrencyCode ?? null,
                numberOfItemsShipped: order.NumberOfItemsShipped ?? 0,
                numberOfItemsUnshipped: order.NumberOfItemsUnshipped ?? 0,
                lastSyncAt: new Date(),
            },
        });
    }
    async syncOrderItems(amazonOrderId) {
        const allItems = [];
        let nextToken;
        do {
            const response = nextToken
                ? await this.spApiClient.getOrderItemsNextPage(amazonOrderId, nextToken)
                : await this.spApiClient.getOrderItems(amazonOrderId);
            if (response.OrderItems)
                allItems.push(...response.OrderItems);
            nextToken = response.NextToken;
        } while (nextToken);
        for (const item of allItems) {
            await prisma.orderItem.upsert({
                where: { amazonOrderId_orderItemId: { amazonOrderId, orderItemId: item.OrderItemId } },
                create: {
                    amazonOrderId,
                    orderItemId: item.OrderItemId,
                    sellerSKU: item.SellerSKU,
                    title: item.Title ?? null,
                    quantityOrdered: item.QuantityOrdered,
                    quantityShipped: item.QuantityShipped,
                    itemPrice: item.ItemPrice ? parseFloat(item.ItemPrice.Amount) : null,
                    currencyCode: item.ItemPrice?.CurrencyCode ?? null,
                    fulfillmentChannel: item.FulfillmentChannel ?? null,
                    isGift: item.IsGift ?? false,
                    conditionId: item.ConditionId ?? null,
                    conditionNote: item.ConditionNote ?? null,
                },
                update: {
                    sellerSKU: item.SellerSKU,
                    title: item.Title ?? null,
                    quantityOrdered: item.QuantityOrdered,
                    quantityShipped: item.QuantityShipped,
                    itemPrice: item.ItemPrice ? parseFloat(item.ItemPrice.Amount) : null,
                    currencyCode: item.ItemPrice?.CurrencyCode ?? null,
                    fulfillmentChannel: item.FulfillmentChannel ?? null,
                    isGift: item.IsGift ?? false,
                    conditionId: item.ConditionId ?? null,
                    conditionNote: item.ConditionNote ?? null,
                },
            });
        }
        return allItems.length;
    }
}
exports.OrderSyncService = OrderSyncService;
//# sourceMappingURL=order-sync.js.map