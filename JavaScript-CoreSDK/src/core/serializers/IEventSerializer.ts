import { EventData } from '../EventData';

export interface IEventSerializer {
    serialize(events: EventData[]): string;
}
