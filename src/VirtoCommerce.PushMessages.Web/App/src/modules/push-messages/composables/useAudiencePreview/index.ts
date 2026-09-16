import { ref, Ref } from "vue";
import { useApiClient, useAsync } from "@vc-shell/framework";

import {
  PushMessageAudienceCriteria,
  PushMessageAudienceResult,
  PushMessageClient,
} from "../../../../api_client/virtocommerce.pushmessages";

const { getApiClient } = useApiClient(PushMessageClient);

export interface AudiencePreviewPayload {
  memberQuery?: string;
  memberIds?: string[];
}

export interface IUseAudiencePreview {
  preview: Ref<PushMessageAudienceResult | undefined>;
  refresh: (payload?: AudiencePreviewPayload) => Promise<void>;
  countFor: (memberId: string) => Promise<number>;
  /** A page of the resolved recipients, for looking before sending. */
  fetchPage: (payload: AudiencePreviewPayload, take?: number) => Promise<PushMessageAudienceResult>;
  loading: Readonly<Ref<boolean>>;
}

/**
 * Asks the platform how many people an audience definition resolves to. The endpoint runs the
 * same funnel the send job runs, so this number is the number that will be sent to.
 */
export function useAudiencePreview(): IUseAudiencePreview {
  const preview = ref<PushMessageAudienceResult>();

  /**
   * Requests can overtake each other — the chip counts hit the same endpoint, and a small
   * audience answers faster than a large one. Only the newest request may set the estimate,
   * otherwise a stale reply shows a number that belongs to an audience the author has changed.
   */
  let latest = 0;

  const { action: refresh, loading } = useAsync<AudiencePreviewPayload>(async (payload) => {
    const ticket = ++latest;

    if (!payload?.memberQuery && !payload?.memberIds?.length) {
      preview.value = undefined;
      return;
    }

    const result = await request(payload.memberQuery, payload.memberIds);

    if (ticket === latest) {
      preview.value = result;
    }
  });

  function fetchPage(payload: AudiencePreviewPayload, take = 50): Promise<PushMessageAudienceResult> {
    return request(payload.memberQuery, payload.memberIds, take);
  }

  async function countFor(memberId: string): Promise<number> {
    const result = await request(undefined, [memberId]);

    return result.totalCount ?? 0;
  }

  async function request(memberQuery?: string, memberIds?: string[], take = 0): Promise<PushMessageAudienceResult> {
    const apiClient = await getApiClient();

    return apiClient.previewRecipients({
      memberQuery,
      memberIds,
      skip: 0,
      take,
    } as PushMessageAudienceCriteria);
  }

  return {
    preview,
    refresh,
    countFor,
    fetchPage,
    loading,
  };
}
