#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <time.h>
#include <sys/ioctl.h> // for ioctl() and TIOCGWINSZ
#include <unistd.h> // for STDOUT_FILENO

#include "sds.c"
#include "vec.c"

#define UP_ESCAPE "\x1b[1A"
#define RETURN_TO_START_ESCAPE "\r"

typedef sds string;

void tryPrintLines(string filename, unsigned long lineCount)
{
	FILE* read;
	string currentLine = sdsempty();
	string* lines = vector_create();
	size_t currentLength = 0;
	ssize_t readResult;

	if ((read = fopen(filename, "r")) != NULL)
	{
		while ((readResult = getline(&currentLine, &currentLength, read)) != -1) {
			vector_add(&lines, sdsnew(currentLine));
		}

		lineCount = (unsigned long)fmin(lineCount, vector_size(lines));

		for (unsigned long i = vector_size(lines) - lineCount; i < vector_size(lines); i++)
		{
			printf("%s", lines[i]);
		}

		for (unsigned long i = 0; i < vector_size(lines); i++)
		{
			sdsfree(lines[i]);
		}
		fclose(read);
	}
}

int getLineOffset(size_t stringLength)
{
	struct winsize consoleSize;
	ioctl(STDOUT_FILENO, TIOCGWINSZ, &consoleSize);

	double consoleColumns = consoleSize.ws_col;

	return (int)(ceil(stringLength / consoleColumns));
}

string intToString(int i)
{
	string result;
	sprintf(result, "%d", i);
	return result;
}

string formatTime(time_t time)
{
	struct tm* timeinfo = localtime(&time);
	char result[80];
	strftime(result, sizeof(result), "%a %Y-%m-%d %H:%M:%S %Z", timeinfo);

	return sdsnew(result);
}

int main(int argc, char* argv[])
{
	const string prompt = sdsnew("terminote> ");
	const string defaultFilename = sdsnew("terminote.log");
	string timestamp = sdsempty();
	string currentEntry = sdsempty();

	const unsigned long linesToShow = 10;

	string filename = sdsnew(defaultFilename);
	string userInput;
	size_t userInputLength = 0;
	ssize_t userInputReadResult = 0;

	if (argc == 1)
	{
		printf("%s\n", defaultFilename);
	}
	else if (argc == 2) {
		filename = argv[1];
	}
	else {
		printf("%s\n", "Usage: terminote [filename]");
	}

	tryPrintLines(filename, linesToShow);

	FILE* notesFile;

	if ((notesFile = fopen(filename, "a+")) != NULL)
	{
		while (1)
		{
			printf("%s", prompt);

			userInputReadResult = getline(&userInput, &userInputLength, stdin);

			if (strcmp(userInput, ":q\n") == 0)
			{
				fclose(notesFile);
				return EXIT_SUCCESS;
			}

			time_t now;
			time(&now);
			timestamp = sdscat(formatTime(now), "> ");
			currentEntry = sdscat(timestamp, userInput);

			for (int i = 0; i < getLineOffset(sdslen(prompt) + sdslen(userInput)); i++)
			{
				printf("%s", UP_ESCAPE);
			}

			printf("%s", RETURN_TO_START_ESCAPE);

			// write to file

			fprintf(notesFile, "%s", currentEntry);

			// show entry on screen

			printf("%s", currentEntry);
		}
	}

	return EXIT_SUCCESS;
}
