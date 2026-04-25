#!/bin/bash
echo "=== Starting CanPany API ==="
(cd /app/api && dotnet CanPany.Api.dll) &
API_PID=$!

echo "=== Starting CanPany Worker ==="
(cd /app/worker && dotnet CanPany.Worker.dll) &
WORKER_PID=$!

echo "Both services running — API: $API_PID | Worker: $WORKER_PID"

wait -n $API_PID $WORKER_PID
EXIT_CODE=$?

echo "A service exited ($EXIT_CODE), shutting down..."
kill $API_PID $WORKER_PID 2>/dev/null
exit $EXIT_CODE
