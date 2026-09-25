import { ref } from "vue";
import { beforeEach, describe, expect, it, vi } from "vitest";

const previewRecipients = vi.fn();

vi.mock("@vc-shell/framework", () => ({
  useApiClient: () => ({ getApiClient: async () => ({ previewRecipients }) }),
  // Enough of useAsync to drive the composable: run the action, track a loading flag.
  useAsync: (innerAction: (payload?: unknown) => Promise<unknown>) => {
    const loading = ref(false);

    return {
      loading,
      error: ref(null),
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
}));

const { useAudiencePreview } = await import("./index");

describe("useAudiencePreview", () => {
  beforeEach(() => {
    previewRecipients.mockReset();
  });

  it("asks nothing and clears the estimate when the audience is empty", async () => {
    const { preview, refresh } = useAudiencePreview();

    await refresh({ memberQuery: "", memberIds: [] });

    expect(previewRecipients).not.toHaveBeenCalled();
    expect(preview.value).toBeUndefined();
  });

  it("clears a previous estimate once the audience is emptied", async () => {
    previewRecipients.mockResolvedValue({ totalCount: 15 });

    const { preview, refresh } = useAudiencePreview();

    await refresh({ memberQuery: "role:Purchaser" });
    expect(preview.value?.totalCount).toBe(15);

    await refresh({});
    expect(preview.value).toBeUndefined();
  });

  it("requests counters only, never a page of recipients", async () => {
    previewRecipients.mockResolvedValue({ totalCount: 8 });

    const { refresh } = useAudiencePreview();

    await refresh({ memberQuery: "role:Purchaser", memberIds: ["org1"] });

    expect(previewRecipients).toHaveBeenCalledWith({
      memberQuery: "role:Purchaser",
      memberIds: ["org1"],
      skip: 0,
      take: 0,
    });
  });

  it("counts a single company for its chip", async () => {
    previewRecipients.mockResolvedValue({ totalCount: 7 });

    const { countFor } = useAudiencePreview();

    await expect(countFor("vdberg")).resolves.toBe(7);
    expect(previewRecipients).toHaveBeenCalledWith({
      memberQuery: undefined,
      memberIds: ["vdberg"],
      skip: 0,
      take: 0,
    });
  });

  it("keeps the newest estimate when an older request answers later", async () => {
    // A big audience is slow; a small one answers first. The slow reply must not win.
    previewRecipients
      .mockImplementationOnce(async () => {
        await new Promise((r) => setTimeout(r, 40));
        return { totalCount: 99 };
      })
      .mockImplementationOnce(async () => ({ totalCount: 3 }));

    const { preview, refresh } = useAudiencePreview();

    const slow = refresh({ memberQuery: "big" });
    const fast = refresh({ memberQuery: "small" });

    await Promise.all([slow, fast]);

    expect(preview.value?.totalCount).toBe(3);
  });

  it("reports a rejected query as a state, not as an error", async () => {
    // The platform answers 400 for a phrase the search index cannot run.
    previewRecipients.mockRejectedValue(Object.assign(new Error("Bad Request"), { status: 400 }));

    const { preview, failed, refresh } = useAudiencePreview();

    await expect(refresh({ memberQuery: "createddate:[not-a-date TO]" })).resolves.toBeUndefined();
    expect(failed.value).toBe(true);
    expect(preview.value).toBeUndefined();
  });

  it("clears the failure once the audience can be counted again", async () => {
    previewRecipients
      .mockRejectedValueOnce(Object.assign(new Error("Bad Request"), { status: 400 }))
      .mockResolvedValueOnce({ totalCount: 4 });

    const { preview, failed, refresh } = useAudiencePreview();

    await refresh({ memberQuery: "broken" });
    await refresh({ memberQuery: "name:Alex" });

    expect(failed.value).toBe(false);
    expect(preview.value?.totalCount).toBe(4);
  });

  it("reads a missing total as zero", async () => {
    previewRecipients.mockResolvedValue({});

    const { countFor } = useAudiencePreview();

    await expect(countFor("vdberg")).resolves.toBe(0);
  });
});
