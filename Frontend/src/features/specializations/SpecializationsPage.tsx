import { useEffect, useState, type FormEvent } from 'react'
import { Alert, Box, Card, CardContent, CircularProgress, Grid, Stack, Typography } from '@mui/material'
import { Button, Pagination, TextField } from '@mui/material'
import { getSpecializations } from './specializationsApi'
import type { Specialization } from './types'

export function SpecializationsPage() {
  const [specializations, setSpecializations] = useState<Specialization[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(0)
  const [nameFilter, setNameFilter] = useState('')
  const [draftNameFilter, setDraftNameFilter] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const pageSize = 10

  useEffect(() => {
    const controller = new AbortController()

    async function loadSpecializations() {
      setIsLoading(true)
      setError(null)

      try {
        const result = await getSpecializations({
          pageNumber,
          pageSize,
          name: nameFilter || undefined,
          signal: controller.signal,
        })

        if (controller.signal.aborted) {
          return
        }

        setSpecializations(result.items)
        setTotalCount(result.totalCount)
        setTotalPages(result.totalPages)
      } catch (caughtError) {
        if (controller.signal.aborted) {
          return
        }

        const message =
          caughtError instanceof Error
            ? caughtError.message
            : 'Failed to load specializations.'
        setError(message)
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    void loadSpecializations()

    return () => {
      controller.abort()
    }
  }, [nameFilter, pageNumber])

  const handleApplyFilter = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setPageNumber(1)
    setNameFilter(draftNameFilter.trim())
  }

  const handleClearFilter = () => {
    setDraftNameFilter('')
    setNameFilter('')
    setPageNumber(1)
  }

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" gutterBottom>
          Specializations
        </Typography>
      </Box>

      <Box
        component="form"
        onSubmit={handleApplyFilter}
        sx={{
          display: 'flex',
          flexDirection: { xs: 'column', sm: 'row' },
          gap: 2,
          alignItems: { xs: 'stretch', sm: 'center' },
        }}
      >
          <TextField
            label="Filter by name"
            value={draftNameFilter}
            onChange={(event) => setDraftNameFilter(event.target.value)}
            size="small"
            fullWidth
          />
          <Button type="submit" variant="outlined">
            Apply
          </Button>
          <Button type="button" variant="text" onClick={handleClearFilter}>
            Clear
          </Button>
      </Box>

      {isLoading ? (
        <CircularProgress />
      ) : null}

      {error ? <Alert severity="error">{error}</Alert> : null}

      {!isLoading && !error ? (
        <Stack spacing={2}>
          <Typography variant="body2" color="text.secondary">
            Total: {totalCount}
          </Typography>
          {specializations.length > 0 ? (
            <Stack spacing={2}>
              <Grid container spacing={2}>
                {specializations.map((specialization) => (
                  <Grid key={specialization.id} size={{ xs: 12, sm: 6, md: 4 }}>
                    <Card variant="outlined">
                      <CardContent>
                        <Box>
                          <Typography variant="h6">
                            {specialization.name}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {specialization.code ?? 'No code'}
                          </Typography>
                        </Box>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>

              {totalPages > 1 ? (
                <Pagination
                  count={totalPages}
                  page={pageNumber}
                  onChange={(_, value) => setPageNumber(value)}
                  color="primary"
                />
              ) : null}
            </Stack>
          ) : (
            <Alert severity="info">
              No specializations found.
            </Alert>
          )}
        </Stack>
      ) : null}
    </Stack>
  )
}
