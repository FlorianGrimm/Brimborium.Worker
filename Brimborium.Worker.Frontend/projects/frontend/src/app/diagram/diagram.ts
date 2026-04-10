import { ChangeDetectionStrategy, Component, viewChild } from '@angular/core';
import { FCanvasComponent, FFlowModule, FMoveNodesEvent } from '@foblex/flow';

@Component({
  selector: 'app-diagram',
  templateUrl: './diagram.html',
  styleUrl: './diagram.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [FFlowModule],
})
export class Diagram {
  private readonly _canvas = viewChild.required(FCanvasComponent);

  constructor(){

  }

  protected loaded(): void {
    this._canvas().resetScaleAndCenter(false);
  }

}
