import defaultConfig, { content } from "@vc-shell/framework/tailwind.config";

export default {
  prefix: "tw-",
  content: [...content, "./src/**/*.{vue,js,ts,jsx,tsx}"],
  theme: {
    ...defaultConfig?.theme,
    fontFamily: {
      sans: ["Roboto", "ui-sans-serif", "system-ui", "sans-serif"]
      // Add any other families you need here
    },
  },
};