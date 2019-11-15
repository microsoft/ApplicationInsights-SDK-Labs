import { ICollector } from './ICollector';
import { Logger } from '../Logger';
import { EventData } from '../EventData';

export class ExceptionCollector implements ICollector {
    private logger: Logger;
    private enabled = true;

    initialize(logger: Logger): void {
        this.logger = logger;

        window.addEventListener("error", this.onError);
    }

    pause() {
        this.enabled = false;
    }

    resume() {
        this.enabled = true;
    }

    private onError(this: any, error: ErrorEvent): boolean {
        if (!this.enabled) {
            return false;
        }

        // TODO: we need to find a better way to initialize this object. 
        // question - do we want to keep one EventData object for all SDKs and use a serializer (and validator) 
        // to translate it to a specific schema? 
        let event = new EventData(error.message);
        event.Properties['ai.exception.stack'] = error.error.stack;

        this.logger.logEvent(event);

        return false;
    }
}