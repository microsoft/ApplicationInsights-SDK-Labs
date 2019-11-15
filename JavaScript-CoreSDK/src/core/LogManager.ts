import { Logger } from './Logger';

export class LogManager {

    private static _loggers: { [token: string]: Logger } = {};

    /**
    * Initialize the log manager. After this method is called events are
    * accepted for transmission.
    * @param {string} tenantToken - The default tenant token.
    * @param {object} config - [Optional] Configuration settings for initialize.
    */
    static initialize(tenantToken: string, configuration: object /* AWTLogConfiguration */) {
    }

    /**
     * Asynchronously sends events currently in the queue. New events added
     * will be sent after the current flush finishes. The passed callback will
     * be called when flush is finished.
     * Note: If LogManager is paused or flush is called again in less than 30 sec
     * then flush will be no-op and the callback will not be called.
     * @param {function} callback - The function to be called when flush finishes.
     * @param {boolean} isAsync - flush is async by default, if set to `false` it will be blocking (sync)
     */
    static flush(callback: () => void, isAsync = true) {
    }

    static flushAndTeardown() {};

    /**
     * Pauses the transmission of events.
     */
    static pauseTransmission() {};

    /**
     * Resumes the transmission of events.
     */
    static resumeTransmission() {};

    // TODO: setTransmitProfile, loadTransmitProfiles 

    /**
     * Set context that will be sent with every event. Also allows marking the value as PII or Customer Content.
     * @param {string} name - Name of the context property.
     * @param {string|number|boolean|AWTEventProperty} value - Value of the context property.
     */
    static setContext(name: string, value: string | number | Boolean  /* AWTEventProperty */) {
        for (let key in LogManager._loggers) {
            LogManager._loggers[key].setContext(name, value);
        }
    }

    /**
     * Gets the logger for the specified tenant token.
     * @param {string} tenantToken - The tenant token for the logger.
     * @return Logger which will send data with the tenant token specified. If tenant token is
     * undefined or null or empty, undefined is returned instead.
     */
    static getLogger(tenantToken?: string): Logger {
        return LogManager._loggers[tenantToken];
    }

    /**
     * Adds the logger
     * @param tenantToken The tenant token for the logger
     * @param logger Logger which will send data with the tenant token specified
     */
    static addLogger(tenantToken: string, logger: Logger) {
        LogManager._loggers[tenantToken] = logger;
    }

    /**
     * Adds a notification listener. The SDK will call methods on the listener
     * when an appropriate notification is raised.
     * @param {object} listener - The notification listener to be added.
     */
    static addNotificationListener(listener: object /* AWTNotificationListener */) {
    };
}
