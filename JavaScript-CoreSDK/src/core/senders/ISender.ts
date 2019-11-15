import { EventData } from '../EventData';

export interface ISender {

    // TODO: consider using IEvent instead of EventData
    send(payload: string);
}