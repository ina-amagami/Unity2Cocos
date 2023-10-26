import { _decorator, Component, CCFloat, Camera, Vec3, math, Quat, input, Input, KeyCode, EventTouch, EventKeyboard } from 'cc';
const { ccclass, property } = _decorator;

@ccclass('SimpleCameraController')
export class SimpleCameraController extends Component {

    @property({ type: CCFloat })
    private speed: number = 3.5;

    @property({ type: CCFloat })
    private touchSensitivity : number = 20;

    private deltaTime: number;
    private isDragging: boolean;

    private velocity: Vec3 = new Vec3();

    start () {
        input.on(Input.EventType.TOUCH_START, this.onTouchStart, this);
        input.on(Input.EventType.TOUCH_END, this.onTouchUp, this);
        input.on(Input.EventType.TOUCH_MOVE, this.onTouchMove, this);
        input.on(Input.EventType.KEY_DOWN, this.onKeyDown, this);
        input.on(Input.EventType.KEY_UP, this.onKeyUp, this);
    }

    update (deltaTime: number) {
        this.deltaTime = deltaTime;
        this.node.translate(this.velocity);
    }

    onTouchStart(event: EventTouch) {
        this.isDragging = true;
    }

    onTouchUp(event: EventTouch) {
        this.isDragging = false;
    }

    onTouchMove(event: EventTouch) {
        if (!this.isDragging) {
            return;
        }
        const quat = new Quat();
        const delta = event.touch.getDelta();
        Quat.fromEuler(quat,
            delta.y * this.touchSensitivity * this.deltaTime,
            -delta.x * this.touchSensitivity * this.deltaTime, 0);
        this.node.rotate(quat);
    }

    onKeyDown(event: EventKeyboard) {
        switch (event.keyCode) {
            case KeyCode.KEY_A:
                this.velocity.x = -this.speed * this.deltaTime;
                break;
            case KeyCode.KEY_D:
                this.velocity.x = this.speed * this.deltaTime;
                break;
            case KeyCode.KEY_W:
                this.velocity.z = -this.speed * this.deltaTime;
                break;
            case KeyCode.KEY_S:
                this.velocity.z = this.speed * this.deltaTime;
                break;
            case KeyCode.KEY_Q:
                this.velocity.y = -this.speed * this.deltaTime;
                break;
            case KeyCode.KEY_E:
                this.velocity.y = this.speed * this.deltaTime;
                break;
            default:
                break;
        }
    }

    onKeyUp(event: EventKeyboard) {
        switch (event.keyCode) {
            case KeyCode.KEY_W:
            case KeyCode.KEY_S:
                this.velocity.z = 0;
                break;
            case KeyCode.KEY_Q:
            case KeyCode.KEY_E:
                this.velocity.y = 0;
                break;
            case KeyCode.KEY_A:
            case KeyCode.KEY_D:
                this.velocity.x = 0;
                break;
            default:
                break;
        }
    }
}
