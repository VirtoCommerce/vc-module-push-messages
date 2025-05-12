/* eslint-disable */
import { CoreBladeAdditionalSettings } from "@vc-shell/framework";
import { Ref } from "vue";

declare module "*.vue" {
  import type { DefineComponent } from "vue";
  const component: DefineComponent<{}, {}, any>;
  export default component;
}

declare module "@vue/runtime-core" {
  interface ComponentCustomProperties extends _ComponentCustomProperties {
    $mergeLocaleMessage: Composer<{}, {}, {}, string, never, string>["mergeLocaleMessage"];
    $hasAccess: (permissions: string | string[] | undefined) => boolean;
    $isPhone: Ref<boolean>;
    $isTablet: Ref<boolean>;
    $isMobile: Ref<boolean>;
    $isDesktop: Ref<boolean>;
    $isTouch: boolean;
    $t: (key: string, ...args: any[]) => string;
    $dynamicModules: {
      [x: string]: {
        components?: { [key: string]: Component };
        composables?: { [key: string]: (...args: any[]) => any };
        default: { install: (app: any, options?: any) => void };
        schema: { [key: string]: DynamicGridSchema | DynamicDetailsSchema };
        locales: { [key: string]: { [key: string]: string } };
        notificationTemplates?: { [key: string]: Component };
      };
    };
  }

  interface ComponentOptionsBase extends CoreBladeAdditionalSettings {}
}

export {};
