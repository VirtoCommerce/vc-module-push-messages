import { defineConfig } from "vitest/config";

// The modules under test are pure functions over strings, so no DOM is needed.
export default defineConfig({
  test: {
    environment: "node",
    include: ["src/**/*.test.ts"],
  },
});
