import { Pipe, PipeTransform } from '@angular/core';
import { getStatusOsInfo, StatusOsInfo } from '../utils/status-os.utils';

@Pipe({
  name: 'statusOs',
  standalone: true
})
export class StatusOSPipe implements PipeTransform {
  transform(status: number | string): StatusOsInfo {
    return getStatusOsInfo(status);
  }
}
