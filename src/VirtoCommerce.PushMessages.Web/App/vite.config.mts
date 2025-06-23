import { getApplicationConfiguration } from "@vc-shell/config-generator";
import { resolve } from "node:path";

export default getApplicationConfiguration({
  resolve: {
    alias: {
      "vee-validate": resolve(__dirname, "node_modules/vee-validate/dist/vee-validate.mjs"),
    },
  },
});