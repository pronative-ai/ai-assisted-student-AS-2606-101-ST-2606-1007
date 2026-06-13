import { SpApiConfig, SpApiOrdersResponse, SpApiOrderItemsResponse } from '../types';
export declare class SpApiClient {
    private config;
    private accessToken;
    private tokenExpiresAt;
    private client;
    constructor(config: SpApiConfig);
    private getLwaAccessToken;
    private ensureValidToken;
    getUnshippedOrders(createdAfter: string, maxResultsPerPage?: number): Promise<SpApiOrdersResponse>;
    getOrdersNextPage(nextToken: string): Promise<SpApiOrdersResponse>;
    getOrderItems(orderId: string): Promise<SpApiOrderItemsResponse>;
    getOrderItemsNextPage(orderId: string, nextToken: string): Promise<SpApiOrderItemsResponse>;
}
//# sourceMappingURL=amazon-sp-api.d.ts.map