import { SpApiClient } from './amazon-sp-api';
import { SyncResult } from '../types';
export declare class OrderSyncService {
    private spApiClient;
    constructor(spApiClient: SpApiClient);
    syncUnshippedOrders(): Promise<SyncResult>;
    private getLastSyncTime;
    private upsertOrder;
    private syncOrderItems;
}
//# sourceMappingURL=order-sync.d.ts.map