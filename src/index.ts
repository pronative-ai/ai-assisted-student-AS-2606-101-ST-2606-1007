import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';

dotenv.config();

import { createRouter } from './api/routes';
import { PORT } from './config';
import logger from './services/logger';

const app = express();

app.use(cors());
app.use(express.json());

app.use('/api/v1', createRouter());

app.use((err: Error, _req: express.Request, res: express.Response, _next: express.NextFunction) => {
  logger.error('Unhandled error', { error: err.message, stack: err.stack });
  res.status(500).json({ status: 'error', message: err.message || 'Internal server error' });
});

app.listen(PORT, () => {
  logger.info(`FBM Fulfillment Agent API running on port ${PORT}`);
  logger.info(`Health check: http://localhost:${PORT}/api/v1/health`);
});

export default app;
