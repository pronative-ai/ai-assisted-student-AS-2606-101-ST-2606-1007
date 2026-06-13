import axios, { AxiosInstance } from 'axios';
import { SpApiConfig, SpApiOrdersResponse, SpApiOrderItemsResponse } from '../types';
import logger from './logger';

const SP_API_ENDPOINT = 'https://sellingpartnerapi-eu.amazon.com';

export class SpApiClient {
  private config: SpApiConfig;
  private accessToken: string | null = null;
  private tokenExpiresAt: number = 0;
  private client: AxiosInstance;

  constructor(config: SpApiConfig) {
    this.config = config;
    this.client = axios.create({
      baseURL: SP_API_ENDPOINT,
      timeout: 30000,
    });

    this.client.interceptors.request.use(async (req) => {
      await this.ensureValidToken();
      req.headers['x-amz-access-token'] = this.accessToken;
      return req;
    });
  }

  private async getLwaAccessToken(): Promise<{ access_token: string; expires_in: number }> {
    const payload = new URLSearchParams({
      grant_type: 'refresh_token',
      refresh_token: this.config.refreshToken,
      client_id: this.config.clientId,
      client_secret: this.config.clientSecret,
    });

    logger.info('Requesting LWA access token');
    const response = await axios.post(
      'https://api.amazon.com/auth/o2/token',
      payload.toString(),
      { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
    );

    return response.data;
  }

  private async ensureValidToken(): Promise<void> {
    if (this.accessToken && Date.now() < this.tokenExpiresAt) {
      return;
    }

    try {
      const tokenData = await this.getLwaAccessToken();
      this.accessToken = tokenData.access_token;
      this.tokenExpiresAt = Date.now() + (tokenData.expires_in - 60) * 1000;
      logger.info('LWA access token refreshed');
    } catch (error: any) {
      logger.error('Failed to obtain LWA access token', {
        error: error.message,
        status: error.response?.status,
      });
      throw new Error(`SP-API auth failed: ${error.message}`);
    }
  }

  async getUnshippedOrders(
    createdAfter: string,
    maxResultsPerPage: number = 100
  ): Promise<SpApiOrdersResponse> {
    logger.info('Fetching unshipped orders', { createdAfter });

    const response = await this.client.get('/orders/v0/orders', {
      params: {
        MarketplaceIds: this.config.marketplaceId,
        CreatedAfter: createdAfter,
        OrderStatuses: 'Unshipped',
        FulfillmentChannels: 'MFN',
        MaxResultsPerPage: maxResultsPerPage,
      },
    });

    return response.data.payload as SpApiOrdersResponse;
  }

  async getOrdersNextPage(nextToken: string): Promise<SpApiOrdersResponse> {
    logger.info('Fetching next page of orders');
    const response = await this.client.get('/orders/v0/orders', {
      params: { NextToken: nextToken },
    });
    return response.data.payload as SpApiOrdersResponse;
  }

  async getOrderItems(orderId: string): Promise<SpApiOrderItemsResponse> {
    logger.info('Fetching order items', { orderId });
    const response = await this.client.get(`/orders/v0/orders/${orderId}/orderItems`);
    return response.data.payload as SpApiOrderItemsResponse;
  }

  async getOrderItemsNextPage(orderId: string, nextToken: string): Promise<SpApiOrderItemsResponse> {
    const response = await this.client.get(
      `/orders/v0/orders/${orderId}/orderItems`,
      { params: { NextToken: nextToken } }
    );
    return response.data.payload as SpApiOrderItemsResponse;
  }
}
