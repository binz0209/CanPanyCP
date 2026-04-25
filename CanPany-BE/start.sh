#!/bin/bash
echo "=== Starting CanPany API ==="
dotnet /app/api/CanPany.Api.dll &
API_PID=$!

echo "=== Starting CanPany Worker ==="
dotnet /app/worker/CanPany.Worker.dll &
WORKER_PID=$!

echo "Both services running — API: $API_PID | Worker: $WORKER_PID"

wait -n $API_PID $WORKER_PID
EXIT_CODE=$?

echo "A service exited ($EXIT_CODE), shutting down..."
kill $API_PID $WORKER_PID 2>/dev/null
exit $EXIT_CODE
