import { EventData } from './EventData';
import { ISender } from './senders/ISender';
import { IEventSerializer } from './serializers/IEventSerializer';
import { IEventHandler } from './eventHandlers';

export class TransmissionChannel {

    private sender: ISender;
    private serializer: IEventSerializer;
    private eventHandlers: Array<IEventHandler> = [];

    // TODO: a really simple queue
    private queue: Array<EventData>;

    constructor(sender: ISender, serializer: IEventSerializer, eventHandlers?: Array<IEventHandler>) {
        this.sender = sender;

        if (eventHandlers) {
            this.eventHandlers = eventHandlers;
        }

        this.queue = [];

        // TODO: allow different profiles and triggering policies. 
        setInterval(this.triggerSend, 15 * 1000);
    }

    addEventHandler(eventHandler: (event: EventData) => boolean | void) {
        this.eventHandlers.push(eventHandler);
    }

    logEvent(event: EventData) {
        this.queue.push(event);
    }

    private triggerSend() {
        if (this.queue.length > 0) {
            // execute all event handlers
            // TODO: allow the handler to reject an item (the same way as telemetryProcessors in JS SDK)
            this.eventHandlers.forEach(handler => {
                this.queue.forEach(event => {
                    handler(event);
                });
            });

            let payload = this.serializer.serialize(this.queue);
            this.sender.send(payload);
        }
    }
}