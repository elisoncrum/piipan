# Building the NAC to protect the PII of SNAP participants

We’re designing the NAC so the system doesn’t store sensitive personally identifiable information (PII) of the ~40 million people applying for or receiving SNAP benefits. That means there will be no database that directly stores SNAP participants’ names, dates of birth, or social security numbers.

A high-level description of our approach is below; a [detailed technical specification](./pprl.md) is also available.

## How are we building the NAC so it doesn’t store PII? 

In order for the NAC to work, states will upload records of active SNAP participants to the NAC on a daily basis. States will use a tool to de-identify the PII of SNAP applicants and participants before uploading this data to the NAC. Each person’s PII is converted to a seemingly-random string of characters that acts as a de-identified code for that individual: the exact same PII always produces the same code. 

<p align="center">
  <a href="./diagrams/combine-and-deidentify.png"><img src="./diagrams/combine-and-deidentify.png" alt="States combine and de-identify participant data"></a>
</p>

States will combine last name, DOB, and SSN into a single field to be de-identified and compared against other codes uploaded to the NAC from other states. States will also upload less-sensitive data associated with the individual (Participant ID, Case ID, recent benefits months), but this will not be used in the match. When two states upload PII records that produce the same code, it will be flagged by the NAC as a match.  

<p align="center">
  <a href="./diagrams/match-process.png"><img src="./diagrams/match-process.png" alt="De-identify query to find match"></a>
</p>

When an exact match of de-identified data is found between states, both states—State 1 (the state that initiated the NAC query) and State 2 (where the match was found)—will be notified and will receive the plain-text data associated with the match. Using this plain-text data (Participant ID, Case ID, Benefits end date, Recent benefits months), both states will work together to resolve the match and take timely action on the case. 

<p align="center">
  <a href="./diagrams/match-result.png"><img src="./diagrams/match-result.png" alt="State sees match in real-time" width="50%"></a>
</p>

As a result, the NAC will not directly store sensitive PII. The de-identification process protects against that SNAP participant PII being exposed or used for purposes other than those specified by the 2018 Farm Bill. It is theoretically possible that a sophisticated attacker could use the information stored in the NAC to extract PII of SNAP participants, but this risk is greatly reduced by combining last name, DOB, and SSN before the de-identification process.

## Privacy-Preserving Record Linkage (PPRL)

This de-identification technique is part of an approach called Privacy-Preserving Record Linkage (PPRL). PPRL is a process that identifies and links records that correspond to the same individual across different databases, without revealing private information to the linking organization. It’s a well-researched approach commonly used by the healthcare industry to keep sensitive medical information secure. (Read more: [Journal of the Medical Informatics Association](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC5009931/))
