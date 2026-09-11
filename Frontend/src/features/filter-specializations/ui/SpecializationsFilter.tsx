import { useState, type SyntheticEvent } from "react";
import {
  Box,
  Button,
  TextField,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
} from "@mui/material";

export interface SpecializationsFilterProps {
  pageSize: number;
  sortOrder: "asc" | "desc";
  name?: string;
  onApplyFilter: (params: {
    name: string;
    pageSize: number;
    sortOrder: "asc" | "desc";
  }) => void;
  onClearFilter: () => void;
}

export function SpecializationsFilter({
  pageSize: initialPageSize,
  sortOrder: initialSortOrder,
  name: initialName = "",
  onApplyFilter,
  onClearFilter,
}: Readonly<SpecializationsFilterProps>) {
  const [draftName, setDraftName] = useState(initialName);
  const [draftPageSize, setDraftPageSize] = useState(initialPageSize);
  const [draftSortOrder, setDraftSortOrder] = useState<"asc" | "desc">(
    initialSortOrder,
  );

  const [prevProps, setPrevProps] = useState({
    name: initialName,
    pageSize: initialPageSize,
    sortOrder: initialSortOrder,
  });

  if (
    prevProps.name !== initialName ||
    prevProps.pageSize !== initialPageSize ||
    prevProps.sortOrder !== initialSortOrder
  ) {
    setPrevProps({
      name: initialName,
      pageSize: initialPageSize,
      sortOrder: initialSortOrder,
    });
    setDraftName(initialName);
    setDraftPageSize(initialPageSize);
    setDraftSortOrder(initialSortOrder);
  }

  const handleSubmit = (event: SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();
    onApplyFilter({
      name: draftName.trim(),
      pageSize: draftPageSize,
      sortOrder: draftSortOrder,
    });
  };

  const handleClear = () => {
    setDraftName("");
    setDraftPageSize(10);
    setDraftSortOrder("asc");
    onClearFilter();
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{ display: "flex", flexDirection: "column", gap: 2 }}
    >
      <Box
        sx={{
          display: "flex",
          flexDirection: { xs: "column", sm: "row" },
          gap: 2,
          alignItems: "center",
        }}
      >
        <TextField
          id="specializations-search-input"
          label="Filter by name"
          value={draftName}
          onChange={(e) => {
            setDraftName(e.target.value);
          }}
          size="small"
          fullWidth
        />

        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel id="specializations-page-size-label">
            Items Per Page
          </InputLabel>
          <Select
            labelId="specializations-page-size-label"
            id="specializations-page-size-select"
            value={draftPageSize}
            label="Items Per Page"
            onChange={(e) => {
              setDraftPageSize(e.target.value);
            }}
          >
            <MenuItem value={5}>5</MenuItem>
            <MenuItem value={10}>10</MenuItem>
            <MenuItem value={25}>25</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel id="specializations-sort-order-label">
            Sort Order
          </InputLabel>
          <Select
            labelId="specializations-sort-order-label"
            id="specializations-sort-order-select"
            value={draftSortOrder}
            label="Sort Order"
            onChange={(e) => {
              setDraftSortOrder(e.target.value);
            }}
          >
            <MenuItem value="asc">Ascending</MenuItem>
            <MenuItem value="desc">Descending</MenuItem>
          </Select>
        </FormControl>
      </Box>

      <Box sx={{ display: "flex", gap: 1, justifyContent: "flex-end" }}>
        <Button type="button" variant="text" onClick={handleClear}>
          Clear
        </Button>
        <Button type="submit" variant="contained">
          Apply Filters
        </Button>
      </Box>
    </Box>
  );
}
