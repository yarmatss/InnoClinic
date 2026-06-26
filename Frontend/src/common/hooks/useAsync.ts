import { useEffect, useReducer, useCallback } from "react";
import axios from "axios";
import type { ProblemDetails } from "../types/api";

interface UseAsyncOptions {
  enabled?: boolean;
}

interface AsyncState<T> {
  data: T | null;
  isLoading: boolean;
  error: string | null;
  validationErrors: Record<string, string[]> | null;
}

type AsyncAction<T> =
  | { type: "FETCH_INIT" }
  | { type: "FETCH_SUCCESS"; payload: T }
  | {
      type: "FETCH_FAILURE";
      error: string;
      validationErrors?: Record<string, string[]> | null;
    }
  | { type: "SET_DATA"; payload: T | null }
  | { type: "SET_VALIDATION_ERRORS"; payload: Record<string, string[]> | null };

function asyncReducer<T>(
  state: AsyncState<T>,
  action: AsyncAction<T>,
): AsyncState<T> {
  switch (action.type) {
    case "FETCH_INIT":
      return { ...state, isLoading: true, error: null, validationErrors: null };
    case "FETCH_SUCCESS":
      return { ...state, isLoading: false, data: action.payload };
    case "FETCH_FAILURE":
      return {
        ...state,
        isLoading: false,
        error: action.error,
        validationErrors: action.validationErrors ?? null,
      };
    case "SET_DATA":
      return { ...state, data: action.payload };
    case "SET_VALIDATION_ERRORS":
      return { ...state, validationErrors: action.payload };
    default:
      return state;
  }
}

export function useAsync<T>(
  asyncCallback: (signal: AbortSignal) => Promise<T>,
  options: UseAsyncOptions = {},
) {
  const { enabled = true } = options;

  const [state, dispatch] = useReducer(asyncReducer<T>, {
    data: null,
    isLoading: enabled,
    error: null,
    validationErrors: null,
  });

  useEffect(() => {
    if (!enabled) {
      return;
    }

    const controller = new AbortController();

    async function run() {
      dispatch({ type: "FETCH_INIT" });

      try {
        const result = await asyncCallback(controller.signal);

        if (!controller.signal.aborted) {
          dispatch({ type: "FETCH_SUCCESS", payload: result });
        }
      } catch (caughtError: unknown) {
        if (controller.signal.aborted) return;

        if (axios.isAxiosError(caughtError) && caughtError.response?.data) {
          const apiError = caughtError.response.data as ProblemDetails;

          dispatch({
            type: "FETCH_FAILURE",
            error: apiError.detail ?? apiError.title,
            validationErrors: apiError.errors,
          });
        } else {
          const fallbackMessage =
            caughtError instanceof Error
              ? caughtError.message
              : "A critical network communication failure occurred.";

          dispatch({ type: "FETCH_FAILURE", error: fallbackMessage });
        }
      }
    }

    void run();

    return () => {
      controller.abort();
    };
  }, [asyncCallback, enabled]);

  const setData = useCallback((data: T | null) => {
    dispatch({ type: "SET_DATA", payload: data });
  }, []);

  const setValidationErrors = useCallback(
    (errors: Record<string, string[]> | null) => {
      dispatch({ type: "SET_VALIDATION_ERRORS", payload: errors });
    },
    [],
  );

  return {
    ...state,
    setData,
    setValidationErrors,
  };
}
