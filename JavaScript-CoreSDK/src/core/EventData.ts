
export class EventData {
    Name?: string;
    Type?: string;
    Timestamp?: number;
    Properties: { [name: string]: string | number | Boolean /* | AWTEventProperty */} = {};

    constructor(name?: string) {
        this.Name = name;
        this.Timestamp = +new Date();
    }
}