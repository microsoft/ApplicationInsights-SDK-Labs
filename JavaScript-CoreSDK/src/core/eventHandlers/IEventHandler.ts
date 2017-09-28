import { EventData } from '../EventData';

export type IEventHandler = (event: EventData) => boolean | void;
