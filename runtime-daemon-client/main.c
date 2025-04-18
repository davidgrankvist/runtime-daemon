#include <windows.h>
#include <stdio.h>

#define MAX_PAYLOAD 300
#define true 1

static void printHelp() {
    char* helpText = "Launch the daemon:\n"
        "runtime-daemon-d\n"
        "\n"
        "Request daemon to execute an assembly:\n"
        "runtime-daemon exec <assembly path> <optional assembly args>\n";
    printf("%s", helpText);
}

int main(int argc, char* argv[]) {
    // -- parse args --
    if (!(argc >= 3 && strcmp(argv[1], "exec") == 0)) {
        printHelp();
        return 0;
    }

    // -- resolve relative path --

    char* relativePath = argv[2];
    char absolutePath[MAX_PATH];
    DWORD pathLength = GetFullPathNameA(
            relativePath,
            MAX_PATH,
            absolutePath,
            NULL);

    if (pathLength == 0 || pathLength > MAX_PATH) {
        fprintf(stderr, "Error resolving path. Code: %lu\n", GetLastError());
        return 1;
    }

    // -- serialize arguments --

    char payload[MAX_PAYLOAD];

    strncpy(payload, absolutePath, pathLength);
    payload[pathLength] = '|';

    int payloadIndex = pathLength + 1;
    for (int i = 3; i < argc; i++) {
        size_t size = strlen(argv[i]);
        strncpy(payload + payloadIndex, argv[i], size);
        payloadIndex += size;
        payload[payloadIndex++] = '|';
    }
    payload[payloadIndex] = '\n';
    payload[payloadIndex + 1] = '\0';

    // -- send payload to daemon --

    const char* pipeName = "\\\\.\\pipe\\runtime-daemon";
    const char* message = payload;

    printf("Connecting..\n");
    HANDLE hPipe = CreateFileA(
            pipeName,
            GENERIC_READ | GENERIC_WRITE,
            0,
            NULL,
            OPEN_EXISTING,
            0,
            NULL);

    if (hPipe == INVALID_HANDLE_VALUE) {
        fprintf(stderr, "Failed to connect to pipe. Error code: %lu\n", GetLastError());
        return 1;
    }

    DWORD bytesWritten = 0;
    BOOL success = WriteFile(
            hPipe,
            message,
            (DWORD)strlen(message),
            &bytesWritten,
            NULL);

    if (!success) {
        fprintf(stderr, "Failed to write to pipe. Error code: %lu\n", GetLastError());
        CloseHandle(hPipe);
        return 1;
    }

    // -- read response --

    char reply[MAX_PAYLOAD];
    int replyBytes = 0;

    while (true) {
        DWORD bytesRead;
        success = ReadFile(hPipe, reply, sizeof(reply) - 1, &bytesRead, NULL);
        if (!success || bytesRead == 0) {
            printf("Failed to read from pipe or no reply received. Error code: %lu\n", GetLastError());
            CloseHandle(hPipe);
            return 1;
        }

        replyBytes += bytesRead;

        if (reply[replyBytes - 1] == '\n') {
            break; 
        }
    }

    reply[replyBytes] = '\0';

    printf("Server responded with: %s\n", reply);

    CloseHandle(hPipe);
    return 0;
}

