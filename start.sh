#!/bin/bash
# Start API and Worker concurrently
# If either process exits, the container exits (Render will restart it)

echo "Starting CanPany API..."
dotnet /app/api/CanPany.Api.dll &
API_PID=$!

echo "Starting CanPany Worker..."
dotnet /app/worker/CanPany.Worker.dll &
WORKER_PID=$!

echo "Both services started — API PID: $API_PID | Worker PID: $WORKER_PID"

# Wait for either process to exit
wait -n $API_PID $WORKER_PID
EXIT_CODE=$?

echo "One service exited with code $EXIT_CODE, shutting down..."
kill $API_PID $WORKER_PID 2>/dev/null
exit $EXIT_CODE
