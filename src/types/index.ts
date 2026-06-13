export interface SpApiConfig {
  clientId: string;
  clientSecret: string;
  refreshToken: string;
  awsAccessKeyId: string;
  awsSecretAccessKey: string;
  awsRoleArn: string;
  marketplaceId: string;
}

export interface SpApiOrder {
  AmazonOrderId: string;
  PurchaseDate: string;
  OrderStatus: string;
  FulfillmentChannel: string;
  MarketplaceId: string;
  BuyerEmail?: string;
  BuyerName?: string;
  ShippingAddress?: Record<string, unknown>;
  OrderTotal?: {
    Amount: string;
    CurrencyCode: string;
  };
  NumberOfItemsShipped?: number;
  NumberOfItemsUnshipped?: number;
}

export interface SpApiOrderItem {
  OrderItemId: string;
  SellerSKU: string;
  Title?: string;
  QuantityOrdered: number;
  QuantityShipped: number;
  ItemPrice?: {
    Amount: string;
    CurrencyCode: string;
  };
  FulfillmentChannel?: string;
  IsGift?: boolean;
  ConditionId?: string;
  ConditionNote?: string;
}

export interface SpApiOrdersResponse {
  Orders?: SpApiOrder[];
  NextToken?: string;
}

export interface SpApiOrderItemsResponse {
  OrderItems?: SpApiOrderItem[];
  NextToken?: string;
}

export type BatchType = 'SINGLE_SKU_Q1' | 'SINGLE_SKU_Q2' | 'SINGLE_SKU_Q3PLUS' | 'MIXED';

export interface BatchSplitResult {
  orderId: string;
  amazonOrderId: string;
  batchType: BatchType;
  batchGroup: string | null;
  items: Array<{
    orderItemId: string;
    sku: string;
    quantity: number;
  }>;
}

export interface SyncResult {
  syncId: string;
  ordersFound: number;
  ordersSynced: number;
  itemsSynced: number;
  status: 'SUCCESS' | 'PARTIAL' | 'FAILED';
  error?: string;
}
