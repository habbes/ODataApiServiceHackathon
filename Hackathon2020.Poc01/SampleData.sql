USE Hackathon2020Db
GO

SET IDENTITY_INSERT Employees ON;
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (1,'Nancy Davolio', NULL);
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (2,'Andrew Fuller', 1);
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (3,'Janet Leverling', 1);
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (4,'Andrew Cencini', 2);
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (5,'Steven Thorpe', 3);
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (6,'Mariya Sergienko', 2);
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (7,'Laura Giussani', 3);
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (8,'Robert Zare', 2);
INSERT INTO Employees ([Id], [Name], [ManagerId]) VALUES (9,'Michael Neipper', 7);
SET IDENTITY_INSERT Employees OFF;

SET IDENTITY_INSERT Projects ON;
INSERT INTO Projects ([Id], [Name], [ManagerId], [Start], [End]) VALUES (1, 'Electrical Installation', 2, '2020-08-06 00:00:00', '2020-11-13 00:00:00');
INSERT INTO Projects ([Id], [Name], [ManagerId], [Start], [End]) VALUES (2, 'Pipeline Installation', 3, '2020-07-17 00:00:00', '2020-08-11 00:00:00');
SET IDENTITY_INSERT Projects OFF;

SET IDENTITY_INSERT Milestones ON;
INSERT INTO Milestones ([Id], [Name], [ProjectId], [Start], [End]) VALUES (1, 'Mobilization', 1, '2020-08-08 00:00:00', '2020-08-23 00:00:00');
INSERT INTO Milestones ([Id], [Name], [ProjectId], [Start], [End]) VALUES (2, 'Construction', 1, '2020-08-27 00:00:00', '2020-10-17 00:00:00');
INSERT INTO Milestones ([Id], [Name], [ProjectId], [Start], [End]) VALUES (3, 'Site Restoration', 1, '2020-09-18 00:00:00', '2020-10-25 00:00:00');
INSERT INTO Milestones ([Id], [Name], [ProjectId], [Start], [End]) VALUES (4, 'Project Closeout', 1, '2020-10-29 00:00:00', '2020-11-13 00:00:00');
INSERT INTO Milestones ([Id], [Name], [ProjectId], [Start], [End]) VALUES (5, 'Excavation', 2, '2020-07-19 00:00:00', '2020-07-25 00:00:00');
INSERT INTO Milestones ([Id], [Name], [ProjectId], [Start], [End]) VALUES (6, 'Installation', 2, '2020-07-27 00:00:00', '2020-08-03 00:00:00');
INSERT INTO Milestones ([Id], [Name], [ProjectId], [Start], [End]) VALUES (7, 'Refill & Paving', 2, '2020-08-04 00:00:00', '2020-08-11 00:00:00');
INSERT INTO Milestones ([Id], [Name], [ProjectId], [Start], [End]) VALUES (8, 'Project Closeout', 2, '2020-08-11 00:00:00', '2020-08-11 00:00:00');
SET IDENTITY_INSERT Milestones OFF;

SET IDENTITY_INSERT Tasks ON;
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (1, 'Notice to Proceed', 1, NULL, NULL, '2020-08-06 00:00:00', '2020-08-06 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (2, 'Project Start', 1, NULL, NULL, '2020-08-07 00:00:00', '2020-08-07 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (3, 'Mobilize', 1, 1, NULL, '2020-08-08 00:00:00', '2020-08-23 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (4, 'Grade Site', 1, 2, 4, '2020-08-27 00:00:00', '2020-09-06 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (5, 'Set Foundation', 1, 2, 4, '2020-08-27 00:00:00', '2020-09-10 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (6, 'Install Conduit', 1, 2, 4, '2020-09-10 00:00:00', '2020-09-12 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (7, 'Dig Cable Trench', 1, 2, 4, '2020-09-11 00:00:00', '2020-09-17 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (8, 'Erect Steel Structures', 1, 2, 6, '2020-09-13 00:00:00', '2020-09-26 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (9, 'Install Equipment', 1, 2, 6, '2020-09-18 00:00:00', '2020-09-26 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (10, 'Install Grounding', 1, 2, 6, '2020-09-27 00:00:00', '2020-10-01 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (11, 'Install Bus and Jumpers', 1, 2, 6, '2020-09-27 00:00:00', '2020-10-10 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (12, 'Lay Control Cable', 1, 2, 6, '2020-09-27 00:00:00', '2020-10-17 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (13, 'Install Fence', 1, 2, 6, '2020-09-10 00:00:00', '2020-09-19 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (14, 'Remove Equipment', 1, 3, 8, '2020-10-18 00:00:00', '2020-10-25 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (15, 'Lay Stoning', 1, 3, 8, '2020-09-18 00:00:00', '2020-09-19 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (16, 'Lay Roadway', 1, 3, 8, '2020-09-18 00:00:00', '2020-09-24 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (17, 'Project SignOff', 1, 4, NULL, '2020-10-29 00:00:00', '2020-11-13 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (18, 'Start Project', 2, NULL, NULL, '2020-07-17 00:00:00', '2020-07-17 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (19, 'Site Survey', 2, NULL, NULL, '2020-07-17 00:00:00', '2020-07-17 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (20, 'Mobilize on Site', 2, NULL, NULL, '2020-07-18 00:00:00', '2020-07-18 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (21, 'Backhoe Excavate', 2, 5, 5, '2020-07-19 00:00:00', '2020-07-24 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (22, 'Install Shoring', 2, 5, 5, '2020-07-24 00:00:00', '2020-07-25 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (23, 'Common Laborer Excavate', 2, 5, 5, '2020-07-25 00:00:00', '2020-07-25 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (24, 'Install Piping', 2, 6, 7, '2020-07-27 00:00:00', '2020-08-01 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (25, 'Install Couplings', 2, 6, 7, '2020-08-02 00:00:00', '2020-08-02 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (26, 'QA Inspection', 2, 6, 7, '2020-08-03 00:00:00', '2020-08-03 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (27, 'Remove Shoring', 2, 7, 9, '2020-08-04 00:00:00', '2020-08-04 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (28, 'Backfill & Compact', 2, 7, 9, '2020-08-07 00:00:00', '2020-08-10 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (29, 'Asphalt Surface Roadway', 2, 7, 9, '2020-08-11 00:00:00', '2020-08-11 00:00:00');
INSERT INTO Tasks ([Id], [Description], [ProjectId], [MilestoneId], [SupervisorId], [Start], [End]) VALUES (30, 'Project SignOff', 2, 8, NULL, '2020-08-11 00:00:00', '2020-08-11 00:00:00');
SET IDENTITY_INSERT Tasks OFF;

SET IDENTITY_INSERT Products ON;
INSERT INTO Products ([Id], [Name], [UnitPrice]) VALUES (1, 'Microsoft Xbox One X - 1TB Console - Black', 45000);
INSERT INTO Products ([Id], [Name], [UnitPrice]) VALUES (2, 'Microsoft Microsoft Surface Pro 3 36W Power Supply', 5700);
SET IDENTITY_INSERT Products OFF;

SET IDENTITY_INSERT Suppliers ON;
INSERT INTO Suppliers ([Id], [Name], [Website]) VALUES (1, 'ABC Limited', 'www.abc.ltd');
INSERT INTO Suppliers ([Id], [Name], [Website]) VALUES (2, 'XYZ Limited', 'www.xyz.ltd');
SET IDENTITY_INSERT Suppliers OFF;

SET IDENTITY_INSERT Customers ON;
INSERT INTO Customers ([Id], [Name]) VALUES (1, 'General Merchants');
SET IDENTITY_INSERT Customers OFF;