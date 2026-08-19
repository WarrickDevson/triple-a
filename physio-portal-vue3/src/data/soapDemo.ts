import type { SoapNote, SharedReport, OwnerSubjectiveNote } from '../types/soap'

export const DEMO_SOAP_NOTES: Record<number, SoapNote[]> = {
  1: [
    {
      soapNoteId: 101,
      petId: 1,
      physioId: 2,
      physioName: 'Dr. Sarah Jenkins, PT',
      appointmentId: 1,
      sessionDate: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
      subjective:
        'Owner reports Buddy completed 80% of home exercises this week. Shows improved willingness to step onto right hind leg after morning rest. Slight stiffness remained after long walks.',
      objective:
        'Mild palpation tenderness over right stifling joint. Extension ROM at 135 deg (up from 120 deg). Muscle symmetry improving (R thigh girth 38cm vs L thigh 40cm). Moderate stiffness at start of trotting.',
      action:
        'Manual therapy (joint mobilization & myofascial release 15 mins). Laser therapy applied to right stifle (4J/cm2). Performed in-session exercises: 3 sets x 10 reps Cavaletti rails, 2 sets x 30 sec balance board.',
      plan:
        'Continue current home exercise program. Increase Cavaletti rail height by 2cm next week. Re-evaluate stiffness score in 7 days. Frequency: 2x weekly clinical sessions.',
      stiffnessScore: 4,
      painScore: 3,
      lamenessScore: 2,
      customMetrics: [
        { name: 'Stifle Extension ROM', value: 135, minScale: 0, maxScale: 180, unitOrDescriptor: 'deg' },
        { name: 'Right Thigh Girth', value: 38, minScale: 20, maxScale: 60, unitOrDescriptor: 'cm' },
        { name: 'Weight Bearing Balance', value: 75, minScale: 0, maxScale: 100, unitOrDescriptor: '%' },
      ],
      isSharedWithOwner: true,
      sharedAtUtc: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
      createdAtUtc: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
    },
    {
      soapNoteId: 100,
      petId: 1,
      physioId: 2,
      physioName: 'Dr. Sarah Jenkins, PT',
      appointmentId: null,
      sessionDate: new Date(Date.now() - 9 * 24 * 60 * 60 * 1000).toISOString(),
      subjective:
        'Initial assessment session. Owner noted lameness after intense exercise. Difficulty climbing stairs.',
      objective:
        'Marked right hind leg lameness (Grade 3/5). Pain score 6/10 on extension. Reduced ROM at 120 deg.',
      action:
        'Initial diagnostic gait analysis & baseline passive ROM stretching. Hydrotherapy underwater treadmill session (10 mins at 1.5 mph).',
      plan:
        'Initiate Phase 1 rehab plan focusing on joint stability and weight distribution. Weekly clinical visits.',
      stiffnessScore: 7,
      painScore: 6,
      lamenessScore: 3,
      customMetrics: [
        { name: 'Stifle Extension ROM', value: 120, minScale: 0, maxScale: 180, unitOrDescriptor: 'deg' },
        { name: 'Right Thigh Girth', value: 36.5, minScale: 20, maxScale: 60, unitOrDescriptor: 'cm' },
      ],
      isSharedWithOwner: true,
      sharedAtUtc: new Date(Date.now() - 9 * 24 * 60 * 60 * 1000).toISOString(),
      createdAtUtc: new Date(Date.now() - 9 * 24 * 60 * 60 * 1000).toISOString(),
    },
  ],
}

export const DEMO_SHARED_REPORTS: Record<number, SharedReport[]> = {
  1: [
    {
      sharedReportId: 201,
      petId: 1,
      soapNoteId: 101,
      sharedByPhysioId: 2,
      sharedByPhysioName: 'Dr. Sarah Jenkins, PT',
      title: 'SOAP Session Report - Clinical Assessment & Progress',
      reportType: 'SOAP_SESSION',
      summary:
        'Continue current home exercise program. Increase Cavaletti rail height by 2cm next week. Re-evaluate stiffness score in 7 days.',
      sharedAtUtc: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
    },
  ],
}

export const DEMO_OWNER_SUBJECTIVE_NOTES: Record<number, OwnerSubjectiveNote[]> = {
  1: [
    {
      ownerSubjectiveNoteId: 301,
      petId: 1,
      ownerId: 5,
      ownerName: 'John Smith',
      noteDate: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
      notes:
        'Buddy seemed a little hesitant going down the back porch steps yesterday after his walk. He ate all his food and seems happy overall, but still favors the left leg slightly.',
      painObserved: 3,
      energyObserved: 4,
      isReviewed: false,
    },
  ],
}
