"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.createRouter = createRouter;
const express_1 = require("express");
const client_1 = require("@prisma/client");
const amazon_sp_api_1 = require("../services/amazon-sp-api");
const order_sync_1 = require("../services/order-sync");
const batch_split_1 = require("../services/batch-split");
const config_1 = require("../config");
const prisma = new client_1.PrismaClient();
function createRouter() {
    const router = (0, express_1.Router)();
    const config = (0, config_1.getSpApiConfig)();
    const spApi = new amazon_sp_api_1.SpApiClient(config);
    const orderSync = new order_sync_1.OrderSyncService(spApi);
    const batchSplit = new batch_split_1.BatchSplitService();
    // Component 1: Order Sync
    router.post('/sync/orders', async (_req, res) => {
        try {
            const result = await orderSync.syncUnshippedOrders();
            res.json(result);
        }
        catch (error) {
            res.status(500).json({ status: 'FAILED', error: error.message });
        }
    });
    router.get('/sync/logs', async (_req, res) => {
        const logs = await prisma.syncLog.findMany({ orderBy: { startedAt: 'desc' }, take: 50 });
        res.json(logs);
    });
    // Component 2: Orders & Items
    router.get('/orders', async (req, res) => {
        const status = req.query.status;
        const page = parseInt(req.query.page || '1', 10);
        const limit = parseInt(req.query.limit || '50', 10);
        const skip = (page - 1) * limit;
        const where = status ? { orderStatus: status } : {};
        const [orders, total] = await Promise.all([
            prisma.amazonOrder.findMany({
                where,
                include: { items: true, batches: true },
                skip,
                take: limit,
                orderBy: { purchaseDate: 'desc' },
            }),
            prisma.amazonOrder.count({ where }),
        ]);
        res.json({ orders, total, page, limit });
    });
    router.get('/orders/:amazonOrderId', async (req, res) => {
        const order = await prisma.amazonOrder.findUnique({
            where: { amazonOrderId: req.params.amazonOrderId },
            include: { items: true, batches: { include: { items: true } } },
        });
        if (!order)
            return res.status(404).json({ error: 'Order not found' });
        res.json(order);
    });
    // Component 3: Batch Split
    router.post('/batches/split', async (_req, res) => {
        const results = await batchSplit.splitOrders();
        res.json({ message: `Split ${results.length} orders`, batches: results });
    });
    router.get('/batches', async (req, res) => {
        const batchType = req.query.batchType;
        const batches = await batchSplit.getBatchesByType(batchType);
        res.json(batches);
    });
    router.get('/batches/stats', async (_req, res) => {
        const stats = await batchSplit.getBatchStatistics();
        res.json(stats);
    });
    router.get('/health', (_req, res) => {
        res.json({ status: 'ok', timestamp: new Date().toISOString(), version: '1.0.0' });
    });
    return router;
}
//# sourceMappingURL=routes.js.map