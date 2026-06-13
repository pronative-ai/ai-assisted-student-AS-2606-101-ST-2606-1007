"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const express_1 = __importDefault(require("express"));
const cors_1 = __importDefault(require("cors"));
const dotenv_1 = __importDefault(require("dotenv"));
dotenv_1.default.config();
const routes_1 = require("./api/routes");
const config_1 = require("./config");
const logger_1 = __importDefault(require("./services/logger"));
const app = (0, express_1.default)();
app.use((0, cors_1.default)());
app.use(express_1.default.json());
app.use('/api/v1', (0, routes_1.createRouter)());
app.use((err, _req, res, _next) => {
    logger_1.default.error('Unhandled error', { error: err.message, stack: err.stack });
    res.status(500).json({ status: 'error', message: err.message || 'Internal server error' });
});
app.listen(config_1.PORT, () => {
    logger_1.default.info(`FBM Fulfillment Agent API running on port ${config_1.PORT}`);
    logger_1.default.info(`Health check: http://localhost:${config_1.PORT}/api/v1/health`);
});
exports.default = app;
//# sourceMappingURL=index.js.map