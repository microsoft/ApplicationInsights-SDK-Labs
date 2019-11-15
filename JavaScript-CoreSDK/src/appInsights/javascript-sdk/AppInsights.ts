import { LogManager } from '../../core/LogManager';
import { Logger } from '../../core/Logger';
import { TransmissionChannel } from '../../core/TransmissionChannel';
import { EventData } from '../../core/EventData';
import { XmlHttpRequestSender } from '../../core/senders';

import { AiSerializer} from './AiSerializer';

export class AppInsights {

    private loggerName = 'aiLogger';
    private iKey = 'sample-ikey';

    private aiLogger: Logger;

    constructor() {
        let aiSerializer = new AiSerializer();

        // create all objects and assemble the logger yourself
        // other option is to use some sort of factory class or dependance injection container to assemble loggers
        // a default logger might be a good idea too
        let xmlSender = new XmlHttpRequestSender('https://example.com/v2/track');
        let tramissionChannel = new TransmissionChannel(xmlSender, aiSerializer);
        this.aiLogger = new Logger(tramissionChannel);

        LogManager.addLogger('aiLogger', this.aiLogger);
        LogManager.setContext('ai.ikey', this.iKey);
    }

    trackPageView(name?: string, url?: string, properties?: { [name: string]: string }, measurements?: { [name: string]: number }, duration?: number) {
        let event = new EventData(name);
        event.Type = 'pageView';
        event.Timestamp = +Date.now();
        event.Properties['ai.url'] = url;
        event.Properties['ai.pageview.duration'] = duration;

        // TODO: figure out how to pass complex objects
        // event.Properties['ai.properties'] = properties;
        // event.Properties['ai.measurements'] = measurements;

        // AppInsights will only use one logger.
        this.aiLogger.logEvent(event);
    }

    addTelemetryProcessor(processor: (event: EventData) => void) {
        this.aiLogger.getChannel().addEventHandler(processor);
    }
}
