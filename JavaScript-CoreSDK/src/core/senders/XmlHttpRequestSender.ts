import { ISender } from './ISender';
import { INotificationListener } from './INotificationListener';

export class XmlHttpRequestSender implements ISender, INotificationListener {

    private url: string;

    constructor(url: string) {
        this.url = url;
    }

    send(payload: string) {
        // make an XmlHttpRequest and send the payload to this.url
        // call events on success, failure, etc.
    }

    eventsSent(payload: string) { };
    eventsDropped(payload: string, reason: object) { };
    eventsRejected(payload: string, reason: object) { };
    eventsRetrying(payload: string) { };

}