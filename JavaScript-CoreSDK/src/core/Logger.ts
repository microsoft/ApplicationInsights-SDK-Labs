import { EventData } from './EventData';
import { TransmissionChannel } from './TransmissionChannel';

export class Logger {

    private transmissionChannel: TransmissionChannel;
    private context: { [name: string]: string | number | Boolean /* | AWTEventProperty */} = {};

    constructor(transmissionChannel: TransmissionChannel) {
        this.transmissionChannel = transmissionChannel;
    }

    /**
     * Set context that will be sent with every event. Also allows marking the value as PII or Customer Content.
     * @param {string} name                 - Name of the context property.
     * @param {string|number|boolean|AWTEventProperty} value - Value of the context property.
     */
    public setContext(name: string, value: string | number | Boolean /* AWTEventProperty */) {
        this.context[name] = value;
    };

    /**
     * Logs a custom event with the specified name and fields - to track information
     * such as how a particular feature is used.
     * @param {EventProperties} event - Can be either an AWTEventProperties object or an AWTEventData object or an event name.
     */
    public logEvent(event: EventData /* | string */) {
        this.applyContext(event);
        this.transmissionChannel.logEvent(event);
    }

    public getChannel() {
        return this.transmissionChannel;
    }

    private applyContext(event: EventData) {
        for (let key in this.context) {
            event.Properties[key] = this.context[key];
        }
    }
}
