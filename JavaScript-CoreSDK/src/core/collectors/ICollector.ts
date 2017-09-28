import { Logger } from '../Logger';

export interface ICollector {

    // TODO: it should take ILogger instead
    initialize(logger: Logger): void;

    pause(): void;
    resume(): void;
}
