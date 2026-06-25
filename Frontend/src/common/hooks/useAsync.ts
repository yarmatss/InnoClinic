import { useState, useEffect, useCallback } from "react";
import axios from "axios";
import type { ProblemDetails } from "../types/api";

interface UseAsyncOptions {
  enabled?: boolean;
}

export function useAsync<T>(
  asyncCallback: (signal: AbortSignal) => Promise<T>,
  dependencies: unknown[],
  options: UseAsyncOptions = {},
) {
  const { enabled = true } = options;

  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(enabled);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<
    string,
    string[]
  > | null>(null);

  const [prevEnabled, setPrevEnabled] = useState(enabled);
  if (enabled !== prevEnabled) {
    setPrevEnabled(enabled);
    setIsLoading(enabled);
  }

  // eslint-disable-next-line react-hooks/exhaustive-deps, react-hooks/use-memo
  const execute = useCallback(asyncCallback, dependencies);

  useEffect(() => {
    if (!enabled) {
      return;
    }

    const controller = new AbortController();

    async function run() {
      setIsLoading(true);
      setError(null);
      setValidationErrors(null);

      try {
        const result = await execute(controller.signal);

        if (!controller.signal.aborted) {
          setData(result);
        }
      } catch (caughtError: unknown) {
        if (controller.signal.aborted) return;

        if (axios.isAxiosError(caughtError) && caughtError.response?.data) {
          const apiError = caughtError.response.data as ProblemDetails;

          if (apiError.errors) {
            setValidationErrors(apiError.errors);
          }

          setError(apiError.detail ?? apiError.title);
        } else {
          const fallbackMessage =
            caughtError instanceof Error
              ? caughtError.message
              : "A critical network communication failure occurred.";
          setError(fallbackMessage);
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void run();

    return () => {
      controller.abort();
    };
  }, [execute, enabled]);

  return {
    data,
    isLoading,
    error,
    validationErrors,
    setData,
    setValidationErrors,
  };
}
