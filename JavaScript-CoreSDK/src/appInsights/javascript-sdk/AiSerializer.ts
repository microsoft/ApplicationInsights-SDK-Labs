import { IEventSerializer } from '../../core/serializers';
import { EventData } from '../../core/EventData';

export class AiSerializer implements IEventSerializer {
    constructor() { }

    serialize(events: EventData[]): string {
        return "ai objects";
    }

}