import { Router, Request, Response } from 'express';
import { PrismaClient } from '@prisma/client';
import { SpApiClient } from '../services/amazon-sp-api';
import { OrderSyncService } from '../services/order-sync';
import { BatchSplitService } from '../services/batch-split';
import { getSpApiConfig } from '../config';
import logger from '../services/logger';

const prisma = new PrismaClient();

export function createRouter(): Router {
  const router = Router();

  const config = getSpApiConfig();
  const spApi = new SpApiClient(config);
  const orderSync = new OrderSyncService(spApi);
  const batchSplit = new BatchSplitService();

  // Component 1: Order Sync
  router.post('/sync/orders', async (_req: Request, res: Response) => {
    try {
      const result = await orderSync.syncUnshippedOrders();
      res.json(result);
    } catch (error: any) {
      res.status(500).json({ status: 'FAILED', error: error.message });
    }
  });

  router.get('/sync/logs', async (_req: Request, res: Response) => {
    const logs = await prisma.syncLog.findMany({ orderBy: { startedAt: 'desc' }, take: 50 });
    res.json(logs);
  });

  // Component 2: Orders & Items
  router.get('/orders', async (req: Request, res: Response) => {
    const status = req.query.status as string | undefined;
    const page = parseInt((req.query.page as string) || '1', 10);
    const limit = parseInt((req.query.limit as string) || '50', 10);
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

  router.get('/orders/:amazonOrderId', async (req: Request, res: Response) => {
    const order = await prisma.amazonOrder.findUnique({
      where: { amazonOrderId: req.params.amazonOrderId as string },
      include: { items: true, batches: { include: { items: true } } },
    });
    if (!order) return res.status(404).json({ error: 'Order not found' });
    res.json(order);
  });

  // Component 3: Batch Split
  router.post('/batches/split', async (_req: Request, res: Response) => {
    const results = await batchSplit.splitOrders();
    res.json({ message: `Split ${results.length} orders`, batches: results });
  });

  router.get('/batches', async (req: Request, res: Response) => {
    const batchType = req.query.batchType as string | undefined;
    const batches = await batchSplit.getBatchesByType(batchType);
    res.json(batches);
  });

  router.get('/batches/stats', async (_req: Request, res: Response) => {
    const stats = await batchSplit.getBatchStatistics();
    res.json(stats);
  });

  router.get('/health', (_req: Request, res: Response) => {
    res.json({ status: 'ok', timestamp: new Date().toISOString(), version: '1.0.0' });
  });

  return router;
}
