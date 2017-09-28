import { EventData } from '../EventData';

/**
 * Interface for the notification listener.
 * @interface
 * @property {function} eventsSent     - [Optional] Function to be called when events are sent.
 * @property {function} eventsDropped  - [Optional] Function to be called when events are dropped.
 * @property {function} eventsRejected - [Optional] Function to be called when events are rejected.
 * @property {function} eventsRetrying - [Optional] Function to be called when events are retried.
 */
export interface INotificationListener {
    eventsSent?: (payload: string) => void;
    eventsDropped?: (payload: string, reason: object /* AWTEventsDroppedReason */) => void;
    eventsRejected?: (payload: string, reason: object /* AWTEventsRejectedReason */) => void;
    eventsRetrying?: (payload: string) => void;
}
