import { BatchSplitResult } from '../types';
export declare class BatchSplitService {
    splitOrders(): Promise<BatchSplitResult[]>;
    private splitSingleOrder;
    private getBatchTypeForQuantity;
    private createBatch;
    private determineBatchGroup;
    getBatchesByType(batchType?: string): Promise<({
        items: {
            id: string;
            orderItemId: string;
            sku: string;
            quantity: number;
            batchId: string;
        }[];
        order: {
            items: {
                id: string;
                amazonOrderId: string;
                fulfillmentChannel: string | null;
                currencyCode: string | null;
                createdAt: Date;
                updatedAt: Date;
                orderItemId: string;
                sellerSKU: string;
                title: string | null;
                quantityOrdered: number;
                quantityShipped: number;
                itemPrice: number | null;
                isGift: boolean;
                conditionId: string | null;
                conditionNote: string | null;
            }[];
        } & {
            id: string;
            amazonOrderId: string;
            purchaseDate: Date;
            orderStatus: string;
            fulfillmentChannel: string;
            marketplaceId: string;
            buyerEmail: string | null;
            buyerName: string | null;
            shippingAddress: string | null;
            orderTotal: number | null;
            currencyCode: string | null;
            numberOfItemsShipped: number;
            numberOfItemsUnshipped: number;
            lastSyncAt: Date;
            createdAt: Date;
            updatedAt: Date;
        };
    } & {
        id: string;
        status: string;
        amazonOrderId: string;
        createdAt: Date;
        updatedAt: Date;
        batchType: string;
        batchGroup: string | null;
    })[]>;
    getBatchStatistics(): Promise<{
        totalUnshippedOrders: number;
        totalBatchesCreated: number;
        breakdown: Record<string, number>;
    }>;
}
//# sourceMappingURL=batch-split.d.ts.map