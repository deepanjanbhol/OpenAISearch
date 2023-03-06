# Develop Q&A bot based on training documents using OpenAI

This repository demonstrates how to create a Question and Answers co-pilot that can provide precise answers based on a specific set of training documents. The process involves identifying a relevant set of training documents using embedding to compare their similarity with the incoming query. Once a relevant set is identified, the content of those documents is passed on to OpenAI query as context in prompt.

For more details, please read: [Blogpost](https://www.linkedin.com/pulse/build-your-own-qa-bot-openai-deepanjan-bhol%3FtrackingId=1LHsp0sKSjm8UZOmQQPXJg%253D%253D/?trackingId=1LHsp0sKSjm8UZOmQQPXJg%3D%3D)

Please note that other mechanisms can be used to find relevant training documents, such as Azure Search, which allows setting up an index on the training documents to retrieve relevant ones.

## Changes needed to get the code working.

1) Need to add apikey of Azure OpenAI in file OpenAIClient.cs
2) Need to change Azure OpenAI lab endpoint in Search.cs

For more details about how to get started with Azure OpenAI, please start here: https://learn.microsoft.com/en-us/azure/cognitive-services/openai/overview
