import { ICollector } from './ICollector';
import { Logger } from '../Logger';
import { EventData } from '../EventData';

export class AjaxCollector implements ICollector {
    private logger: Logger;

    initialize(logger: Logger): void {
        this.logger = logger;

        // handle ajax request.
    }

    pause() {
    }

    resume() {
    }
}