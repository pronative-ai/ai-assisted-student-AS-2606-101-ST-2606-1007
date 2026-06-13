"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.SpApiClient = void 0;
const axios_1 = __importDefault(require("axios"));
const logger_1 = __importDefault(require("./logger"));
const SP_API_ENDPOINT = 'https://sellingpartnerapi-eu.amazon.com';
class SpApiClient {
    config;
    accessToken = null;
    tokenExpiresAt = 0;
    client;
    constructor(config) {
        this.config = config;
        this.client = axios_1.default.create({
            baseURL: SP_API_ENDPOINT,
            timeout: 30000,
        });
        this.client.interceptors.request.use(async (req) => {
            await this.ensureValidToken();
            req.headers['x-amz-access-token'] = this.accessToken;
            return req;
        });
    }
    async getLwaAccessToken() {
        const payload = new URLSearchParams({
            grant_type: 'refresh_token',
            refresh_token: this.config.refreshToken,
            client_id: this.config.clientId,
            client_secret: this.config.clientSecret,
        });
        logger_1.default.info('Requesting LWA access token');
        const response = await axios_1.default.post('https://api.amazon.com/auth/o2/token', payload.toString(), { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } });
        return response.data;
    }
    async ensureValidToken() {
        if (this.accessToken && Date.now() < this.tokenExpiresAt) {
            return;
        }
        try {
            const tokenData = await this.getLwaAccessToken();
            this.accessToken = tokenData.access_token;
            this.tokenExpiresAt = Date.now() + (tokenData.expires_in - 60) * 1000;
            logger_1.default.info('LWA access token refreshed');
        }
        catch (error) {
            logger_1.default.error('Failed to obtain LWA access token', {
                error: error.message,
                status: error.response?.status,
            });
            throw new Error(`SP-API auth failed: ${error.message}`);
        }
    }
    async getUnshippedOrders(createdAfter, maxResultsPerPage = 100) {
        logger_1.default.info('Fetching unshipped orders', { createdAfter });
        const response = await this.client.get('/orders/v0/orders', {
            params: {
                MarketplaceIds: this.config.marketplaceId,
                CreatedAfter: createdAfter,
                OrderStatuses: 'Unshipped',
                FulfillmentChannels: 'MFN',
                MaxResultsPerPage: maxResultsPerPage,
            },
        });
        return response.data.payload;
    }
    async getOrdersNextPage(nextToken) {
        logger_1.default.info('Fetching next page of orders');
        const response = await this.client.get('/orders/v0/orders', {
            params: { NextToken: nextToken },
        });
        return response.data.payload;
    }
    async getOrderItems(orderId) {
        logger_1.default.info('Fetching order items', { orderId });
        const response = await this.client.get(`/orders/v0/orders/${orderId}/orderItems`);
        return response.data.payload;
    }
    async getOrderItemsNextPage(orderId, nextToken) {
        const response = await this.client.get(`/orders/v0/orders/${orderId}/orderItems`, { params: { NextToken: nextToken } });
        return response.data.payload;
    }
}
exports.SpApiClient = SpApiClient;
//# sourceMappingURL=amazon-sp-api.js.map