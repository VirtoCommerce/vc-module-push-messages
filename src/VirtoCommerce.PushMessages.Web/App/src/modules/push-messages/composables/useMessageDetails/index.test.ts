import { ref } from "vue";
import { beforeEach, describe, expect, it, vi } from "vitest";

const get = vi.fn();

vi.mock("@vc-shell/framework", () => ({
  useApiClient: () => ({ getApiClient: async () => ({ get }) }),
  // Enough of useAsync to drive the composable: run the action, track a loading flag.
  useAsync: (innerAction: (payload?: unknown) => Promise<unknown>) => {
    const loading = ref(false);

    return {
      loading,
      action: async (payload?: unknown) => {
        loading.value = true;
        try {
          return await innerAction(payload);
        } finally {
          loading.value = false;
        }
      },
    };
  },
  useLoading: () => ref(false),
}));

const { useMessageDetails } = await import("./index");

describe("useMessageDetails", () => {
  beforeEach(() => {
    get.mockReset();
    get.mockResolvedValue({ id: "m1", status: "Sent", recipientsTotalCount: 7 });
  });

  it("loads the recipient count a sent message's card shows", async () => {
    const { loadMessage } = useMessageDetails({ id: "m1" });

    await loadMessage();

    const responseGroup = get.mock.calls[0][1] as string;

    expect(responseGroup.split(",")).toEqual(expect.arrayContaining(["WithMembers", "WithReadRate"]));
  });
});
