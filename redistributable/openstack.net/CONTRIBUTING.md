# Contributor Guide

## Open Discussion

The most important part of an open source project is *open discussion*. If a developer, user, or other contributor has
a question, comment, suggestion, or concern about any part of this project, **please speak up**: create an issue or add
a comment to an existing issue or pull request.

This policy especially affects the emoji described below. Any user or contributor may freely use these symbols to
clarify intent during discussions. Questions and concerns from first-time contributors matter just as much as questions
and concerns from the original project developers.

## License

The code in this repository is licensed under the MIT License. A complete description of the license, as well as license
notices for 3rd party code incorporated into this project, is available in the repository:

* SDK 1.x: [License.aml](src/Documentation/Content/License.aml)
* SDK 2.x: [License.aml](src/Docs.OpenStack.Net/Content/License.aml)

## Labels

This project uses many labels for categorizing issues and pull requests.

| Label | Meaning on Issue | Meaning on Pull Request |
| --- | --- | --- |
| bug | The issue concerns a bug in the code | The issue concerns a bug in the code |
| enhancement | The issue is an improvement to the code | The pull request is an improvement to the code |
| question | The issue is a question | Normally not applicable (see comments if observed on a pull request) |
| help wanted | The issue is currently being discussed, and a decision for implementation has not yet been decided | The pull request is waiting on code review(s) |
| pull request | A pull request intended to address the issue has been created, but not yet merged | n/a |
| do not merge | n/a | The pull request should not be merged at this time. This could indicate a work-in-progress, a problem in the implementation code, or cases where the pull request depends on (is blocked by) another issue or pull request which has not been addressed. |
| in progress | A developer is currently working on the issue | A developer is currently making updates to the code in the pull request |
| fixed | The issue has been resolved | The pull request describes a new issue (i.e. no separate issue exists), and the content of the pull request was merged to fix the issue |
| duplicate | Another issue or pull request contains the original report for this topic | Another pull request was submitted to correct the issue. This is generally only applied to pull requests after another pull request to correct the issue is merged. |
| wontfix | The issue will not be corrected. The current behavior could be by design, out of scope, or cannot be changed due to the breaking changes policy for the project (see comments for details). | The pull request will not be merged due to a fundamental issue (see description for this label on issues) |
| v1 | The issue affects the openstack.net V1.x SDK | The pull request affects the openstack.net V1.x SDK |
| v2 | The issue affects the OpenStack.NET V2.x SDK | The pull request affects the OpenStack.NET V2.x SDK |

## Emoji

GitHub provides several emoji which can be included in many areas of the site, especially including comments on issues
and pull requests. To reduce confusion during evaluation of a pull request, the following emoji convey special intent.

* :question: (`:question:`) Indicates a question. This emoji comes with **no strings attached**, which means it's fine to simply answer
  the question in another comment without making any changes to code.

  In general, pull requests with an unanswered question will not be merged until *someone* addresses it. This could be
  the original creator of the issue or pull request, another contributor to the project, or even the person who asked
  the question.

* :exclamation: (`:exclamation:`) Indicates a problem with the code which will need to be addressed before changes can be merged into
  `master`. This *may* be accompanied by a specific suggestion for how to change the code. Any contributor is completely
  welcome to open a discussion regarding alternative solutions, even if the originally proposed change has already been
  made.

  Every active contributor to a project is likely to use the :exclamation: emoji (or similar) due to a simple
  misunderstanding or oversight in their evaluation. **This is absolutely fine.** If you notice a case like this, simply
  add a comment to clarify the intent and the exclamation can be considered resolved.

  Pull requests with an unresolved exclamation will not be merged until they are addressed by either a change in the
  code or a detailed explanation. While it is possible for code with an exclamation to be merged, this will usually only
  occur if *all* of the following conditions hold:

    * The rationale is fully and clearly explained
    * The code corrects an issue that is likely to affect users
    * All other proposed solutions are either too time-consuming to implement or are equally (or more) problematic

* :bulb: (`:bulb:`) Indicates an idea or suggestion regarding a potential change in the code. These items will not necessarily
  block the inclusion of code into the `master` branch, especially in cases where suggestions are trivial and no other
  issues were found during code review.

  In certain trivial cases (e.g. a simple formatting change), a core contributor on the project may make the suggested
  change themselves in the process of merging a pull request. Like the :exclamation: emoji, :bulb: suggestions invite
  open discussion about alternative solutions from any contributor.
