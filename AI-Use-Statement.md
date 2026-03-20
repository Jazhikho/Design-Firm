# AI Use Statement

## Purpose

This document defines how AI may be used in the repository and related project materials.

The goal is not to ban AI assistance. The goal is to keep authorship, technical judgment, source evaluation, validation, and release approval in human hands.

## Core Principle

AI may assist with:

- exploration
- drafting
- transformation
- testing
- critique
- formatting
- structured ideation
- refactoring suggestions
- documentation cleanup

AI may not be treated as the final authority for:

- design decisions
- historical claims
- period-accuracy claims
- social framing
- legal or licensing judgments
- release approval
- publication or repository policy decisions

Human judgment remains mandatory at every decisive stage.

## Repository Standard

AI-assisted work may be kept only when all of the following are true:

1. A human defined the task, constraints, and success criteria.
2. A human critically reviewed the output rather than accepting it automatically.
3. A human revised, integrated, tested, or otherwise substantively transformed the output.
4. A human accepted responsibility for the final result.
5. AI use is disclosed where disclosure is relevant, appropriate, or reasonably expected.

No AI-assisted output may be shipped, published, or merged on the assumption that the model is authoritative.

## Allowed Uses

### Generally Allowed

AI may be used for:

- code scaffolding
- test drafts
- bug-hunting suggestions
- refactoring suggestions
- formatting and cleanup
- documentation drafts and rewrites
- schema comparison
- summary generation from human-authored notes
- structured brainstorming
- UI copy drafts
- export-format drafting

## Restricted Uses

AI must not be treated as the primary authority or autonomous producer for:

- historical interpretation or period-accuracy claims
- claims about course-aligned academic content without human verification against sources
- legal or licensing conclusions
- final curriculum interpretation
- final release notes or product claims without human review
- social modeling without explicit human audit
- citations or bibliographic claims that have not been verified by a human
- content that imitates protected settings, distinctive proprietary material, or copyrighted styles

## Human Review Standard

All meaningful AI-assisted outputs must pass through this chain:

Human task definition -> AI assistance -> human review -> human revision or integration -> validation -> human approval

For code, validation usually means tests, determinism checks, manual verification, or source review.

For fashion-history and period-accuracy material, validation also includes checking the underlying source material in `Docs/Sources.md` and course-approved references.

## Substantive Transformation Rule

AI-assisted output may be incorporated only if it has undergone at least one meaningful human contribution, such as:

- substantial rewriting
- structural reorganization
- integration into a larger human-authored system or workflow
- explicit testing and correction against defined criteria
- source verification against cited or referenced materials
- selection among alternatives using human rationale

Minor wording cleanup alone is not sufficient when the AI contribution is substantial.

## Provenance and Recordkeeping

For each significant AI-assisted artifact, maintain a provenance entry in [AI-Provenance-Log.md](./Docs/AI-Provenance-Log.md) with:

- date
- tool or model used
- task purpose
- input materials used
- summary of AI contribution
- what the human accepted / rejected / changed
- validation method used
- if accepted (final approver)

The record may be brief, but it must be specific enough to demonstrate human direction and review.

## Bias, Slop, and Release Review

Before release, AI-assisted outputs should be checked for:

- hallucinated facts
- unverifiable citations
- shallow or generic prose
- contradictions with era-specific fashion references or client requirements
- cultural flattening or stereotype drift
- overgeneralized or inaccurate claims about historical dress, identity, or culture
- accidental imitation of protected settings or styles
- polished but unusable code or documentation

Outputs that fail these checks must be revised or discarded.

## AI Artifacts vs AI-Assisted Tasks

AI assistance is not treated as equivalent to authorship. Where AI is used for tasks that do not result in the creation of new AI artifacts (such as repo setup, file reorganization, maintenance edits, or code review), it is treated as a non-authorial task and does not need a provenance entry.

AI-assisted contributions that materially shape shipped code, claims, player-facing text, research interpretation, or documented decisions should be logged in `Docs/AI-Provenance-Log.md`.

## Contributor Rules

Contributors using AI must:

- disclose meaningful AI assistance in contribution notes when relevant
- verify code behavior, determinism, and text accuracy
- verify historical and source-based claims against human-reviewed materials
- avoid submitting unverifiable citations or licensing claims
- avoid using AI to imitate proprietary settings or copyrighted styles
- add or update provenance entries for significant AI-assisted artifacts
- keep project docs in sync when AI-assisted work changes requirements, behavior, or references

Contributors may not use AI assistance as an excuse for:

- broken code
- fabricated documentation
- shallow or generic writing
- unverified historical claims
- culturally irresponsible outputs
- licensing contamination

## Review Checklist

Before accepting meaningful AI-assisted material, ask:

- Did a human define the task clearly?
- Was the output reviewed critically rather than accepted by default?
- Was it substantively revised, integrated, tested, or source-checked?
- Does it create licensing, attribution, or provenance gaps?
- Does it introduce bias, stereotype drift, or unsupported period-accuracy claims?
- Is final responsibility clearly human?

If any of the final four answers is problematic, do not ship it.

## Internal Summary

AI may assist, but it may not decide. Human contributors define goals, verify sources, validate behavior, review outputs, approve releases, and accept responsibility for the final work.
