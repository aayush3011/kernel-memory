﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticMemory.Configuration;
using Microsoft.SemanticMemory.Pipeline;
using Microsoft.SemanticMemory.Search;
using Microsoft.SemanticMemory.WebService;

// ReSharper disable once CheckNamespace
namespace Microsoft.SemanticMemory;

public class MemoryService : ISemanticMemoryClient
{
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly SearchClient _searchClient;

    public MemoryService(
        IPipelineOrchestrator orchestrator,
        SearchClient searchClient)
    {
        this._orchestrator = orchestrator ?? throw new ConfigurationException("The orchestrator is NULL");
        this._searchClient = searchClient ?? throw new ConfigurationException("The search client is NULL");
    }

    /// <inheritdoc />
    public async Task<string> ImportDocumentAsync(
        Document document,
        string? index = null,
        IEnumerable<string>? steps = null,
        CancellationToken cancellationToken = default)
    {
        DocumentUploadRequest uploadRequest = await document.ToDocumentUploadRequestAsync(index, steps, cancellationToken).ConfigureAwait(false);
        return await this.ImportDocumentAsync(uploadRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> ImportDocumentAsync(
        string fileName,
        string? documentId = null,
        TagCollection? tags = null,
        string? index = null,
        IEnumerable<string>? steps = null,
        CancellationToken cancellationToken = default)
    {
        var document = new Document(documentId, tags: tags).AddFile(fileName);
        var uploadRequest = await document.ToDocumentUploadRequestAsync(index, steps, cancellationToken).ConfigureAwait(false);
        return await this.ImportDocumentAsync(uploadRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<string> ImportDocumentAsync(
        DocumentUploadRequest uploadRequest,
        CancellationToken cancellationToken = default)
    {
        var index = IndexExtensions.CleanName(uploadRequest.Index);
        return this._orchestrator.ImportDocumentAsync(index, uploadRequest, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> IsDocumentReadyAsync(
        string documentId,
        string? index = null,
        CancellationToken cancellationToken = default)
    {
        index = IndexExtensions.CleanName(index);
        return this._orchestrator.IsDocumentReadyAsync(index: index, documentId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DataPipelineStatus?> GetDocumentStatusAsync(
        string documentId,
        string? index = null,
        CancellationToken cancellationToken = default)
    {
        index = IndexExtensions.CleanName(index);
        return this._orchestrator.ReadPipelineSummaryAsync(index: index, documentId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<SearchResult> SearchAsync(
        string query,
        string? index = null,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        index = IndexExtensions.CleanName(index);
        return this._searchClient.SearchAsync(index: index, query, filter, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MemoryAnswer> AskAsync(
        string question,
        string? index = null,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        index = IndexExtensions.CleanName(index);
        return this._searchClient.AskAsync(index: index, question: question, filter: filter, cancellationToken: cancellationToken);
    }
}