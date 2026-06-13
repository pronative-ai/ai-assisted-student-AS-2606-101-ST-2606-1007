import { PrismaClient } from '@prisma/client';
import { BatchType, BatchSplitResult } from '../types';
import logger from './logger';

const prisma = new PrismaClient();

export class BatchSplitService {
  async splitOrders(): Promise<BatchSplitResult[]> {
    logger.info('Starting batch split process');

    const orders = await prisma.amazonOrder.findMany({
      where: { orderStatus: 'Unshipped', batches: { none: {} } },
      include: { items: true },
    });

    logger.info(`Found ${orders.length} orders to split`);
    const results: BatchSplitResult[] = [];

    for (const order of orders) {
      try {
        const result = await this.splitSingleOrder(order);
        if (result) results.push(result);
      } catch (error: any) {
        logger.error('Failed to split order', {
          amazonOrderId: order.amazonOrderId,
          error: error.message,
        });
      }
    }

    return results;
  }

  private async splitSingleOrder(order: {
    id: string;
    amazonOrderId: string;
    items: Array<{
      id: string;
      orderItemId: string;
      sellerSKU: string;
      quantityOrdered: number;
    }>;
  }): Promise<BatchSplitResult | null> {
    const { amazonOrderId, items } = order;
    if (items.length === 0) return null;

    const uniqueSkus = new Set(items.map((i) => i.sellerSKU));

    if (items.length === 1) {
      return this.createBatch(
        amazonOrderId,
        this.getBatchTypeForQuantity(items[0].quantityOrdered),
        null,
        items
      );
    }

    if (uniqueSkus.size === 1) {
      const totalQty = items.reduce((sum, i) => sum + i.quantityOrdered, 0);
      return this.createBatch(
        amazonOrderId,
        this.getBatchTypeForQuantity(totalQty),
        items[0].sellerSKU,
        items
      );
    }

    return this.createBatch(amazonOrderId, 'MIXED', null, items);
  }

  private getBatchTypeForQuantity(qty: number): BatchType {
    if (qty === 1) return 'SINGLE_SKU_Q1';
    if (qty === 2) return 'SINGLE_SKU_Q2';
    return 'SINGLE_SKU_Q3PLUS';
  }

  private async createBatch(
    amazonOrderId: string,
    batchType: BatchType,
    batchGroup: string | null,
    items: Array<{ id: string; orderItemId: string; sellerSKU: string; quantityOrdered: number }>
  ): Promise<BatchSplitResult> {
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

  private determineBatchGroup(sku: string): string {
    return sku.split('-')[0] || sku;
  }

  async getBatchesByType(batchType?: string) {
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

    const breakdown: Record<string, number> = {};
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
