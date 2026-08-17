import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { initFrontendTracing } from './app/core/telemetry/telemetry.init';

initFrontendTracing();

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));

