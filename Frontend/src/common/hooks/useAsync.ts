import { useState, useEffect, useCallback } from "react";
import type { ProblemDetails } from "../types/api";

interface UseAsyncOptions {
  enabled?: boolean;
}

export function useAsync<T>(
  asyncCallback: (signal: AbortSignal) => Promise<T>,
  dependencies: unknown[],
  options: UseAsyncOptions = {},
) {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [validationErrors, setValidationErrors] = useState<Record<
    string,
    string[]
  > | null>(null);

  const { enabled = true } = options;

  const execute = useCallback(asyncCallback, dependencies);

  useEffect(() => {
    if (!enabled) {
      setIsLoading(false);
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
      } catch (caughtError: any) {
        if (controller.signal.aborted) return;

        if (caughtError.response && caughtError.response.data) {
          const apiError = caughtError.response.data as ProblemDetails;

          if (apiError.errors) {
            setValidationErrors(apiError.errors);
          }

          setError(
            apiError.detail ??
              apiError.title ??
              "A validation failure occurred.",
          );
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
