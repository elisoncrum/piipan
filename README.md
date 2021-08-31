[![Build Status][badge_ci]][1] [![Maintainability][badge_cc_maint]][2] [![Test Coverage][badge_cc_cov]][3]


# 🥧 piipan

*A privacy-preserving system for storing and matching de-identified Personal Identifiable Information (PII) records.*

## Quick links
- [Quickstart Guide for States](https://github.com/18F/piipan/blob/dev/docs/quick-start-guide-states.md)
- [High-level architecture diagram](https://raw.githubusercontent.com/18F/piipan/dev/docs/piipan-architecture.png)

## Overview

Piipan is a reference model for program integrity initiatives that aim to prevent multiple enrollment in federally-funded, but state-managed benefit programs. It is the open-source foundation for the [USDA Food and Nutrition Service](https://www.fns.usda.gov) National Accuracy Clearinghouse (NAC), a congressionally mandated matching system for the [Supplemental Nutrition Assistance Program (SNAP)](https://www.fns.usda.gov/snap/supplemental-nutrition-assistance-program).

Under this model:
1. State eligibility systems share *de-identified* participant data to Piipan daily

<p align="center">
  <a href="./docs/diagrams/daily-snapshots.png"><img src="./docs/diagrams/daily-snapshots.png" alt="De-identified participant data" width="60%"></a>
</p>

2. Duplicate participation is prevented by using Piipan to search for matches during eligibility (re)certification

<p align="center">
  <a href="./docs/diagrams/prevent-duplicate-enrolment.png"><img src="./docs/diagrams/prevent-duplicate-enrolment.png" alt="De-identified participant data" width="80%"></a>
</p>

Paramount quality attributes of this system include:
* Preserving the privacy of program participants
* Accuracy of matches
* Adaptability to multiple benefit programs

[Sec. 4011 of the 2018 Farm Bill](https://www.congress.gov/bill/115th-congress/house-bill/2/text), *Interstate data matching to prevent multiple issuances*, further guides our work, mandating that the information made available by state agencies:
* Shall be used only for the purpose of preventing multiple enrollment
* Shall not be retained for longer than is necessary

To achieve this product vision, Piipan incorporates a Privacy-Preserving Record Linkage (PPRL) technique to de-identify the PII of program participants at the state-level. Please see our [high-level treatment](./docs/pprl-plain.md) and our [technical specification](./docs/pprl.md) for more details.

**Note**: Our documentation will sometimes use the terms Piipan and NAC interchangeably. However, more precisely, Piipan is our [open-source product available on GitHub](https://github.com/18F/piipan), while the NAC is a deployment of that product, configured specifically for the Food and Nutrition Service, and operated under their policies and regulations. 

## Documentation

[High-level architecture](./docs/architecture.md), process, and (sub)system documentation, as well as Architectural Decision Records (ADRs), are organized in [this index](./docs/README.md).

## Development

Piipan is implemented with .NET and Microsoft Azure, using a Platform as a Service (PaaS) and Function as a Service (FaaS) approach. Once Piipan's [prerequisites](./docs/iac.md#prerequisites) are installed in a development environment, its subsystems can be built and tested by navigating to the top of its project tree and running:

```
./build.bash test
```

For more details, see Piipan's [architecture and implementation notes](./docs/architecture.md), our [team practices](./docs/engineering-team-practices.md), and our [other technical documentation](./docs/README.md).

## Public domain

This project is in the worldwide [public domain](LICENSE.md). As stated in [CONTRIBUTING](CONTRIBUTING.md):

> This project is in the public domain within the United States, and copyright
> and related rights in the work worldwide are waived through the [CC0 1.0
> Universal public domain
> dedication](https://creativecommons.org/publicdomain/zero/1.0/).
>
> All contributions to this project will be released under the CC0
>dedication. By submitting a pull request, you are agreeing to comply
>with this waiver of copyright interest.

[badge_ci]: https://circleci.com/gh/18F/piipan.svg?style=shield
[badge_cc_maint]: https://api.codeclimate.com/v1/badges/e14b8f6ac1f5a8e0f5bf/maintainability
[badge_cc_cov]: https://api.codeclimate.com/v1/badges/e14b8f6ac1f5a8e0f5bf/test_coverage
[1]: https://circleci.com/gh/18F/piipan
[2]: https://codeclimate.com/github/18F/piipan/maintainability
[3]: https://codeclimate.com/github/18F/piipan/test_coverage
